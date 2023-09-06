using Engine.Board;
using Engine.Data.Bitboards;
using Engine.Evaluation;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EvaluationTuner;

internal static class InjectableEvaluation
{
    public static int EvaluatePosition( BoardData board, ModifiableEvaluationSheet sheet )
    {
        int totalPhase = 16 * sheet.PiecePhase[0] +
                                       4 * sheet.PiecePhase[1] +
                                       4 * sheet.PiecePhase[2] +
                                       4 * sheet.PiecePhase[3] +
                                       2 * sheet.PiecePhase[4] +
                                       2 * sheet.PiecePhase[5];

        int materialMidEval = 0;
        int materialEndEval = 0;
        int pstsMidEval = 0;
        int pstsEndEval = 0;
        int doublePawnsMidEval = 0;
        int doubledPawnsEndEval = 0;
        int bishopPairMidEval = 0;
        int bishopPairEndEval = 0;
        int phase = totalPhase;

        for (int color = 0; color <= 1; color++)
        {
            uint xorOffset = color == 0 ? 0u : 56u;
            Bitboard piecesBitboardForSide = board.GetPiecesBitboardForSide(color);
            for (uint pieceIndex = 0; pieceIndex < 6; pieceIndex++)
            {
                Bitboard buffer = board.GetPieceBitboard( (int)pieceIndex ) & piecesBitboardForSide;
                phase -= buffer.BitCount * Unsafe.Add( ref MemoryMarshal.GetReference( sheet.PiecePhase.AsSpan() ), pieceIndex );

                Values values = Unsafe.ReadUnaligned<Values>(ref Unsafe.Add(
                    ref Unsafe.As<ushort, byte>(ref MemoryMarshal.GetReference(sheet.PieceValues.AsSpan())),
                    pieceIndex * sizeof(ushort) * 2));

                //material eval
                materialMidEval += values.Midgame * buffer.BitCount;
                materialEndEval += values.Endgame * buffer.BitCount;

                //bishop pair
                if (pieceIndex == 2 && buffer.BitCount >= 2)
                {
                    bishopPairMidEval += sheet.BishopPairMidgameBonus * buffer.BitCount;
                    bishopPairEndEval += sheet.BishopPairEndgameBonus * buffer.BitCount;
                }

                uint pieceIndexOffset = 128u * pieceIndex;

                //psts
                while (buffer != 0)
                {
                    Psts psts = Unsafe.ReadUnaligned<Psts>(ref Unsafe.Add(
                        ref Unsafe.As<sbyte, byte>(ref MemoryMarshal.GetReference(sheet.PstsTable.AsSpan())),
                        pieceIndexOffset + ((uint)buffer.LsbIndex ^ xorOffset) * 2u));
                    pstsMidEval += psts.Midgame;
                    pstsEndEval += psts.Endgame;
                    buffer &= buffer - 1;
                }
            }

            materialMidEval = -materialMidEval;
            materialEndEval = -materialEndEval;

            pstsMidEval = -pstsMidEval;
            pstsEndEval = -pstsEndEval;

            bishopPairMidEval = -bishopPairMidEval;
            bishopPairEndEval = -bishopPairEndEval;
        }

        //penalty for double pawns
        for (int fileIndex = 0; fileIndex < 8; fileIndex++)
        {
            const ulong fileMask = 0x0101010101010101;
            Bitboard fileBuffer = fileMask << fileIndex;


            int whiteDoublePawns = ((Bitboard)(board.GetPieceBitboard(0, 0) & fileBuffer)).BitCount;
            if (whiteDoublePawns > 1)
            {
                doublePawnsMidEval += whiteDoublePawns * sheet.DoublePawnMidgamePenalty;
                doubledPawnsEndEval += whiteDoublePawns * sheet.DoublePawnEndgamePenalty;
            }

            int blackDoublePawns = ((Bitboard)(board.GetPieceBitboard(0, 1) & fileBuffer)).BitCount;
            if (blackDoublePawns > 1)
            {
                doublePawnsMidEval -= blackDoublePawns * sheet.DoublePawnMidgamePenalty;
                doubledPawnsEndEval -= blackDoublePawns * sheet.DoublePawnEndgamePenalty;
            }
        }

        int midgame = materialMidEval + pstsMidEval + doublePawnsMidEval + bishopPairMidEval;
        int endgame = materialEndEval + pstsEndEval + doubledPawnsEndEval + bishopPairEndEval;

        phase = (phase * 256 + totalPhase / 2) / totalPhase;
        return (midgame * (256 - phase) + endgame * phase) / 256 * (board.SideToMove == 0 ? 1 : -1);
    }

    [StructLayout( LayoutKind.Sequential )]
    private readonly struct Psts
    {
        public readonly sbyte Midgame;
        public readonly sbyte Endgame;
    }

    [StructLayout( LayoutKind.Sequential )]
    private readonly struct Values
    {
        public readonly ushort Midgame;
        public readonly ushort Endgame;
    }
}

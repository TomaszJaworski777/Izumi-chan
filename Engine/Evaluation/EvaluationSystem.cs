using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Engine.Board;
using Engine.Data.Bitboards;

namespace Engine.Evaluation;
public class EvaluationSystem
{
    private readonly int _totalPhase = 16 * EvaluationSheet.PiecePhase[0] +
                                       4 * EvaluationSheet.PiecePhase[1] +
                                       4 * EvaluationSheet.PiecePhase[2] +
                                       4 * EvaluationSheet.PiecePhase[3] +
                                       2 * EvaluationSheet.PiecePhase[4] +
                                       2 * EvaluationSheet.PiecePhase[5];

    public int EvaluatePosition( BoardData board, bool debug = false )
    {
        int materialMidEval = 0;
        int materialEndEval = 0;
        int pstsMidEval = 0;
        int pstsEndEval = 0;
        int doublePawnsMidEval = 0;
        int doubledPawnsEndEval = 0;
        int phase = _totalPhase;

        for (int color = 0; color <= 1; color++)
        {
            uint xorOffset = color == 0 ? 0u : 56u;
            Bitboard piecesBitboardForSide = board.GetPiecesBitboardForSide(color);
            for (uint pieceIndex = 0; pieceIndex < 6; pieceIndex++)
            {
                Bitboard buffer = board.GetPieceBitboard( (int)pieceIndex ) & piecesBitboardForSide;
                phase -= buffer.BitCount * Unsafe.Add(ref MemoryMarshal.GetReference(EvaluationSheet.PiecePhase), pieceIndex);

                Values values = Unsafe.ReadUnaligned<Values>(ref Unsafe.Add(
                    ref Unsafe.As<ushort, byte>(ref MemoryMarshal.GetReference(EvaluationSheet.PieceValues)),
                    pieceIndex * sizeof(ushort) * 2));

                //material eval
                materialMidEval += values.Midgame * buffer.BitCount;
                materialEndEval += values.Endgame * buffer.BitCount;

                uint pieceIndexOffset = 64u * pieceIndex;

                while (buffer != 0)
                {
                    Psts psts = Unsafe.ReadUnaligned<Psts>(ref Unsafe.Add(
                        ref Unsafe.As<sbyte, byte>(ref MemoryMarshal.GetReference(EvaluationSheet.PstsTable)),
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
        }

        //pawn bonuses/punishments
        for (int fileIndex = 0; fileIndex < 8; fileIndex++)
        {
            const ulong fileMask = 0x0101010101010101;
            Bitboard fileBuffer = fileMask << fileIndex;

            //penalty for double pawns
            int whiteDoublePawns = ((Bitboard)(board.GetPieceBitboard(0, 0) & fileBuffer)).BitCount;
            if (whiteDoublePawns > 1)
            {
                doublePawnsMidEval += whiteDoublePawns * EvaluationSheet.DoublePawnMidgamePunishment;
                doubledPawnsEndEval += whiteDoublePawns * EvaluationSheet.DoublePawnEndgamePunishment;
            }

            int blackDoublePawns = ((Bitboard)(board.GetPieceBitboard(0, 1) & fileBuffer)).BitCount;
            if (blackDoublePawns > 1)
            {
                doublePawnsMidEval -= blackDoublePawns * EvaluationSheet.DoublePawnMidgamePunishment;
                doubledPawnsEndEval -= blackDoublePawns * EvaluationSheet.DoublePawnEndgamePunishment;
            }
        }

        if (debug)
        {
            Console.WriteLine( $"Material Midgame Evaluation: {materialMidEval}" );
            Console.WriteLine( $"Material Endgame Evaluation: {materialEndEval}" );
            Console.WriteLine( $"PSTS Midgame Evaluation: {pstsMidEval}" );
            Console.WriteLine( $"PSTS Endgame Evaluation: {pstsEndEval}" );
            Console.WriteLine( $"Double pawns Midgame Evaluation: {doublePawnsMidEval}" );
            Console.WriteLine( $"Double pawns Endgame Evaluation: {doubledPawnsEndEval}" );
        }

        int midgame = materialMidEval + pstsMidEval + doublePawnsMidEval;
        int endgame = materialEndEval + pstsEndEval + doubledPawnsEndEval;

        phase = (phase * 256 + _totalPhase / 2) / _totalPhase;
        return (midgame * (256 - phase) + endgame * phase) / 256 * (board.SideToMove == 0 ? 1 : -1);
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct Psts
    {
        public readonly sbyte Midgame;
        public readonly sbyte Endgame;
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct Values
    {
        public readonly ushort Midgame;
        public readonly ushort Endgame;
    }
}

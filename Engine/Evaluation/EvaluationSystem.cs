using Engine.Board;
using Engine.Data.Bitboards;
using Engine.Data.Enums;
using System.Runtime.CompilerServices;

namespace Engine.Evaluation;
public class EvaluationSystem
{
    private readonly int _totalPhase = 16 * EvaluationSheet.PiecePhase[0] +
                                       4 * EvaluationSheet.PiecePhase[1] +
                                       4 * EvaluationSheet.PiecePhase[2] +
                                       4 * EvaluationSheet.PiecePhase[3] +
                                       2 * EvaluationSheet.PiecePhase[4] +
                                       2 * EvaluationSheet.PiecePhase[5];

    [MethodImpl( MethodImplOptions.AggressiveOptimization )]
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
            for (int pieceIndex = 0; pieceIndex < 6; pieceIndex++)
            {
                Bitboard buffer = board.GetPieceBitboard( pieceIndex ) & board.GetPiecesBitboardForSide(color);
                phase -= buffer.BitCount * EvaluationSheet.PiecePhase[pieceIndex];
                bool isWhitePiece = color == 0;

                while (buffer > 0)
                {
                    int squareIndex = buffer.LsbIndex;
                    buffer &= buffer - 1;

                    if (!isWhitePiece)
                        squareIndex ^= 56;

                    //material eval
                    materialMidEval += EvaluationSheet.MidgamePieceValues[pieceIndex];
                    materialEndEval += EvaluationSheet.EndgamePieceValues[pieceIndex];

                    //PSTS eval
                    (int pstsMid, int pstsEnd) = GetPstsValue( (PieceType)pieceIndex, squareIndex );
                    pstsMidEval += pstsMid;
                    pstsEndEval += pstsEnd;
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

    private static (int, int) GetPstsValue( PieceType pieceType, int squareIndex ) => pieceType switch
    {
        PieceType.Pawn => (EvaluationSheet.MidgamePawnTable[squareIndex], EvaluationSheet.EndgamePawnTable[squareIndex]),
        PieceType.Knight => (EvaluationSheet.MidgameKnightTable[squareIndex], EvaluationSheet.EndgameKnightTable[squareIndex]),
        PieceType.Bishop => (EvaluationSheet.MidgameBishopTable[squareIndex], EvaluationSheet.EndgameBishopTable[squareIndex]),
        PieceType.Rook => (EvaluationSheet.MidgameRookTable[squareIndex], EvaluationSheet.EndgameRookTable[squareIndex]),
        PieceType.Queen => (EvaluationSheet.MidgameQueenTable[squareIndex], EvaluationSheet.EndgameQueenTable[squareIndex]),
        PieceType.King => (EvaluationSheet.MidgameKingTable[squareIndex], EvaluationSheet.EndgameKingTable[squareIndex]),
        _ => (0, 0)
    };
}

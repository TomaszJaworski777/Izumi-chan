using Engine.Board;
using Engine.Data.Bitboards;
using Engine.Data.Enums;
using System.Runtime.CompilerServices;

namespace Engine.Evaluation;
public class EvaluationSystem
{
    private const ulong _fileMask = 0x0101010101010101;

    private readonly EvaluationSheet _sheet;
    private readonly int _totalPhase;

    public EvaluationSystem()
    {
        _sheet = new();

        _totalPhase = 16 * _sheet.PiecePhase[0] +
                      4 * _sheet.PiecePhase[1] +
                      4 * _sheet.PiecePhase[2] +
                      4 * _sheet.PiecePhase[3] +
                      2 * _sheet.PiecePhase[4] +
                      2 * _sheet.PiecePhase[5];
    }

    public EvaluationSystem( EvaluationSheet injectedSheet )
    {
        _sheet = injectedSheet;

        _totalPhase = 16 * _sheet.PiecePhase[0] +
              4 * _sheet.PiecePhase[1] +
              4 * _sheet.PiecePhase[2] +
              4 * _sheet.PiecePhase[3] +
              2 * _sheet.PiecePhase[4] +
              2 * _sheet.PiecePhase[5];
    }

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
                phase -= buffer.BitCount * _sheet.PiecePhase[pieceIndex];
                bool isWhitePiece = color == 0;

                while (buffer > 0)
                {
                    int squareIndex = buffer.LsbIndex;
                    buffer &= buffer - 1;

                    if (!isWhitePiece)
                        squareIndex ^= 56;

                    //material eval
                    materialMidEval += _sheet.MidgamePieceValues[pieceIndex];
                    materialEndEval += _sheet.EndgamePieceValues[pieceIndex];

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
/*        Bitboard fileBuffer;
        for (int fileIndex = 0; fileIndex < 8; fileIndex++)
        {
            fileBuffer = _fileMask << fileIndex;

            //penalty for double pawns
            int whiteDoublePawns = ((Bitboard)(board.GetPieceBitboard(0, 0) & fileBuffer)).BitCount;
            if(whiteDoublePawns > 1)
            {
                doublePawnsMidEval += whiteDoublePawns * _sheet.DoublePawnMidgamePunishment;
                doubledPawnsEndEval += whiteDoublePawns * _sheet.DoublePawnEndgamePunishment;
            }

            int blackDoublePawns = ((Bitboard)(board.GetPieceBitboard(0, 1) & fileBuffer)).BitCount;
            if(blackDoublePawns > 1)
            {
                doublePawnsMidEval -= blackDoublePawns * _sheet.DoublePawnMidgamePunishment;
                doubledPawnsEndEval -= blackDoublePawns * _sheet.DoublePawnEndgamePunishment;
            }
        }*/

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

    private (int, int) GetPstsValue( PieceType pieceType, int squareIndex ) => pieceType switch
    {
        PieceType.Pawn => (_sheet.MidgamePawnTable[squareIndex], _sheet.EndgamePawnTable[squareIndex]),
        PieceType.Knight => (_sheet.MidgameKnightTable[squareIndex], _sheet.EndgameKnightTable[squareIndex]),
        PieceType.Bishop => (_sheet.MidgameBishopTable[squareIndex], _sheet.EndgameBishopTable[squareIndex]),
        PieceType.Rook => (_sheet.MidgameRookTable[squareIndex], _sheet.EndgameRookTable[squareIndex]),
        PieceType.Queen => (_sheet.MidgameQueenTable[squareIndex], _sheet.EndgameQueenTable[squareIndex]),
        PieceType.King => (_sheet.MidgameKingTable[squareIndex], _sheet.EndgameKingTable[squareIndex]),
        _ => (0, 0)
    };
}

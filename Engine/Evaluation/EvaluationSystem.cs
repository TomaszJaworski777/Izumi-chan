using Engine.Board;
using Engine.Data.Bitboards;
using Engine.Data.Enums;
using System.Runtime.CompilerServices;

namespace Engine.Evaluation;
public class EvaluationSystem
{
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

    public EvaluationSystem(EvaluationSheet injectedSheet)
    {
        _sheet = injectedSheet;

        _totalPhase = 16 * _sheet.PiecePhase[0] +
              4 * _sheet.PiecePhase[1] +
              4 * _sheet.PiecePhase[2] +
              4 * _sheet.PiecePhase[3] +
              2 * _sheet.PiecePhase[4] +
              2 * _sheet.PiecePhase[5];
    }

    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public int EvaluatePosition( BoardData board )
    {
        int phase = _totalPhase;
        int midgame = 0;
        int endgame = 0;

        Bitboard whitePieces = board.GetPiecesBitboardForSide(0);
        for (int pieceIndex = 0; pieceIndex < 6; pieceIndex++)
        {
            Bitboard buffer = board.GetPieceBitboard( pieceIndex );
            phase -= buffer.BitCount * _sheet.PiecePhase[pieceIndex];

            while (buffer > 0)
            {
                int squareIndex = buffer.LsbIndex;
                buffer &= buffer - 1;

                int sign = whitePieces.GetBitValue(squareIndex) > 0 ? 1 : -1;
                midgame += sign * _sheet.MidgamePieceValues[pieceIndex];
                endgame += sign * _sheet.MidgamePieceValues[pieceIndex];

                if (sign < 0)
                    squareIndex ^= 56;

                switch ((PieceType)pieceIndex)
                {
                    case PieceType.Pawn:
                        midgame += sign * _sheet.MidgamePawnTable[squareIndex];
                        endgame += sign * _sheet.EndgamePawnTable[squareIndex];
                        break;
                    case PieceType.Knight:
                        midgame += sign * _sheet.MidgameKnightTable[squareIndex];
                        endgame += sign * _sheet.EndgameKnightTable[squareIndex];
                        break;
                    case PieceType.Bishop:
                        midgame += sign * _sheet.MidgameBishopTable[squareIndex];
                        endgame += sign * _sheet.EndgameBishopTable[squareIndex];
                        break;
                    case PieceType.Rook:
                        midgame += sign * _sheet.MidgameRookTable[squareIndex];
                        endgame += sign * _sheet.EndgameRookTable[squareIndex];
                        break;
                    case PieceType.Queen:
                        midgame += sign * _sheet.MidgameQueenTable[squareIndex];
                        endgame += sign * _sheet.EndgameQueenTable[squareIndex];
                        break;
                    case PieceType.King:
                        midgame += sign * _sheet.MidgameKingTable[squareIndex];
                        endgame += sign * _sheet.EndgameKingTable[squareIndex];
                        break;
                }
            }
        }

        phase = (phase * 256 + _totalPhase / 2) / _totalPhase;
        return (midgame * (256 - phase) + endgame * phase) / 256 * (board.SideToMove == 0 ? 1 : -1);
    }
}

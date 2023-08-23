using Izumi.Helpers;
using Izumi.Structures;
using Izumi.Structures.Data;

namespace Izumi.Systems
{
    internal static class FenSystem
    {
        public static void CreateBoard(ref Board board, string fen)
        {
            string[] fenParts = fen.Split(' ');
            string[] positionSegments = fenParts[0].Split('/');

            for (int rank = 0; rank < positionSegments.Length; rank++)
            {
                for (int file = 0, index = 0; file < 8; file++, index++)
                {
                    int squareIndex = (7 - rank) * 8 + file;
                    char currentCharacter = positionSegments[rank][index];

                    if (int.TryParse(currentCharacter.ToString(), out int output))
                    {
                        file += output - 1;
                        continue;
                    }

                    bool isWhite = currentCharacter < 'a';

                    switch (currentCharacter)
                    {
                        case 'p':
                        case 'P':
                            board.SetPieceOnSquare(PieceType.Pawn, isWhite, squareIndex);
                            break;
                        case 'n':
                        case 'N':
                            board.SetPieceOnSquare(PieceType.Knight, isWhite, squareIndex);
                            break;
                        case 'b':
                        case 'B':
                            board.SetPieceOnSquare(PieceType.Bishop, isWhite, squareIndex);
                            break;
                        case 'r':
                        case 'R':
                            board.SetPieceOnSquare(PieceType.Rook, isWhite, squareIndex);
                            break;
                        case 'q':
                        case 'Q':
                            board.SetPieceOnSquare(PieceType.Queen, isWhite, squareIndex);
                            break;
                        case 'k':
                        case 'K':
                            board.SetPieceOnSquare(PieceType.King, isWhite, squareIndex);
                            break;
                    }
                }
            }

            board.IsWhiteToMove = fenParts[1] == "w";

            for (int i = 0; i < fenParts[2].Length; i++)
            {
                if (fenParts[2][i] == '-')
                    break;
                else if (fenParts[2][i] == 'Q')
                    board.CanWhiteCastleQueenSide = true;
                else if (fenParts[2][i] == 'K')
                    board.CanWhiteCastleKingSide = true;
                else if (fenParts[2][i] == 'q')
                    board.CanBlackCastleQueenSide = true;
                else if (fenParts[2][i] == 'k')
                    board.CanBlackCastleKingSide = true;
            }

            board.WhiteKingInCheck = board.IsKingInCheck( true );
            board.BlackKingInCheck = board.IsKingInCheck( false );

            if (fenParts[3] == "-")
                board.EnPassantSquareIndex = 0;
            else
                board.EnPassantSquareIndex = new Square(fenParts[3]).SquareIndex;

            if (int.TryParse(fenParts[4], out int moveCount ))
                board.HalfMoves = moveCount;

            if (int.TryParse(fenParts[5], out moveCount))
                board.Moves = moveCount;

            board.ZobristKey = ZobristHashing.GenerateKey(board);
        }
    }
}

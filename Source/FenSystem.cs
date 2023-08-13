namespace Greg
{
    internal static class FenSystem
    {
        public static void CreateBoard( ref Board board, string fen )
        {
            string[] fenParts = fen.Split(' ');
            string[] positionSegments = fenParts[0].Split('/');

            board.Data[12] = new();
            board.Data[13] = new();

            for (int rank = 0; rank < positionSegments.Length; rank++)
            {
                for (int file = 0; file < 8; file++)
                {
                    int squareIndex = (7 - rank) * 8 + file;
                    char currentCharacter = positionSegments[rank][file];

                    if (int.TryParse( currentCharacter.ToString(), out int output ))
                    {
                        file += output - 1;
                        continue;
                    }

                    bool isWhite = currentCharacter < 'a';

                    board.Data[isWhite ? 12 : 13].SetBitToOne( squareIndex );

                    switch (currentCharacter)
                    {
                        case 'p':
                        case 'P':
                            board.SetPieceOnSquare( PieceType.Pawn, isWhite, squareIndex );
                            break;
                        case 'n':
                        case 'N':
                            board.SetPieceOnSquare( PieceType.Knight, isWhite, squareIndex );
                            break;
                        case 'b':
                        case 'B':
                            board.SetPieceOnSquare( PieceType.Bishop, isWhite, squareIndex );
                            break;
                        case 'r':
                        case 'R':
                            board.SetPieceOnSquare( PieceType.Rook, isWhite, squareIndex );
                            break;
                        case 'q':
                        case 'Q':
                            board.SetPieceOnSquare( PieceType.Queen, isWhite, squareIndex );
                            break;
                        case 'k':
                        case 'K':
                            board.SetPieceOnSquare( PieceType.King, isWhite, squareIndex );
                            break;
                    }
                }
            }

            board.IsWhiteToMove = fenParts[1] == "w";

            board.Data[16].SetBitToZero( 0 );
            board.Data[16].SetBitToZero( 1 );
            board.Data[16].SetBitToZero( 2 );
            board.Data[16].SetBitToZero( 3 );
            for (int i = 0; i < fenParts[2].Length; i++)
            {
                if (fenParts[2][i] == '-')
                    break;
                else if (fenParts[2][i] == 'Q')
                    board.Data[16].SetBitToOne( 0 );
                else if (fenParts[2][i] == 'K')
                    board.Data[16].SetBitToOne( 1 );
                else if (fenParts[2][i] == 'q')
                    board.Data[16].SetBitToOne( 2 );
                else if (fenParts[2][i] == 'k')
                    board.Data[16].SetBitToOne( 3 );
            }

            if (fenParts[3] == "-")
                board.EnPassantSquareIndex = 0;
            else
                board.EnPassantSquareIndex = (ulong)new Square( fenParts[3] ).SquareIndex;

            if (ulong.TryParse( fenParts[4], out ulong moveCount ))
                board.HalfMoves = moveCount;

            if (ulong.TryParse( fenParts[5], out moveCount ))
                board.Moves = moveCount;

            board.Data[14] = board.Data[12] | board.Data[13];
        }
    }
}

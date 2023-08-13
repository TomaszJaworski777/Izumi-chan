namespace Greg
{
    internal static class MoveController
    {
        public static bool MakeMove( ref Board originalBoard, Move move )
        {
            Board board = originalBoard;

            int fromIndex = move.From;
            int toIndex = move.To;
            PieceType movingPiece = move.MovingPiece;
            PieceType targetPiece = move.TargetPiece;
            PieceType promotionPiece = move.PromotionPiece;
            bool isCapture = move.IsCapture;
            bool isCastle = move.IsCastle;
            bool isEnPassant = move.IsEnPassant;
            bool isPromotion = move.IsPromotion;
            bool isWhiteToMove = board.IsWhiteToMove;
            int currentSideAllPieceIndex = isWhiteToMove ? 12 : 13;

            ref Bitboard movingPieceBitboard = ref board.Data[GetPieceIndex(movingPiece, isWhiteToMove)];
            movingPieceBitboard.SetBitToZero( fromIndex );

            if (!isPromotion)
                movingPieceBitboard.SetBitToOne( toIndex );
            else
                board.Data[GetPieceIndex( promotionPiece, isWhiteToMove )].SetBitToOne( toIndex );

            if (isCapture)
            {
                if (isEnPassant)
                {
                    board.Data[GetPieceIndex( targetPiece, !isWhiteToMove )].SetBitToZero( toIndex + (isWhiteToMove ? -8 : 8) );
                    board.Data[isWhiteToMove ? 13 : 12].SetBitToZero( toIndex + (isWhiteToMove ? -8 : 8) );
                } else
                {
                    board.Data[GetPieceIndex( targetPiece, !isWhiteToMove )].SetBitToZero( toIndex );
                    board.Data[isWhiteToMove ? 13 : 12].SetBitToZero( toIndex );
                }
            }

            if (isCastle)
            {
                bool kingSide = toIndex - fromIndex == 2;
                if (kingSide)
                {
                    board.Data[GetPieceIndex( PieceType.Rook, isWhiteToMove )].SetBitToZero( toIndex + 1 );
                    board.Data[GetPieceIndex( PieceType.Rook, isWhiteToMove )].SetBitToOne( toIndex - 1 );
                    board.Data[currentSideAllPieceIndex].SetBitToZero( toIndex + 1 );
                    board.Data[currentSideAllPieceIndex].SetBitToOne( toIndex - 1 );
                } else
                {
                    board.Data[GetPieceIndex( PieceType.Rook, isWhiteToMove )].SetBitToZero( toIndex - 2 );
                    board.Data[GetPieceIndex( PieceType.Rook, isWhiteToMove )].SetBitToOne( toIndex + 1 );
                    board.Data[currentSideAllPieceIndex].SetBitToZero( toIndex - 2 );
                    board.Data[currentSideAllPieceIndex].SetBitToOne( toIndex + 1 );
                }
            }

            board.Data[14] = PieceAttack.GenerateAttackBitboard( true );
            board.Data[15] = PieceAttack.GenerateAttackBitboard( false );

            board.Data[currentSideAllPieceIndex].SetBitToZero( fromIndex );
            board.Data[currentSideAllPieceIndex].SetBitToOne( toIndex );

            if (board.IsKingInCheck(isWhiteToMove))
                return false;

            board.IsWhiteToMove = !isWhiteToMove;

            if (movingPiece == PieceType.King)
            {
                board.Data[16].SetBitToZero( isWhiteToMove ? 0 : 2 );
                board.Data[16].SetBitToZero( isWhiteToMove ? 1 : 3 );
            }

            if (movingPiece == PieceType.Rook)
            {
                if(fromIndex == (isWhiteToMove ? 0 : 56))
                    board.Data[16].SetBitToZero( isWhiteToMove ? 0 : 2 );
                else if (fromIndex == (isWhiteToMove ? 7 : 63))
                    board.Data[16].SetBitToZero( isWhiteToMove ? 1 : 3 );
            }

            if (targetPiece == PieceType.Rook)
            {
                if (toIndex == (!isWhiteToMove ? 0 : 56))
                    board.Data[16].SetBitToZero( !isWhiteToMove ? 0 : 2 );
                else if (toIndex == (!isWhiteToMove ? 7 : 63))
                    board.Data[16].SetBitToZero( !isWhiteToMove ? 1 : 3 );
            }

            if (movingPiece == PieceType.Pawn && Math.Abs( fromIndex - toIndex ) == 16)
                board.EnPassantSquareIndex = (ulong)(fromIndex + (isWhiteToMove ? 8 : -8));
            else
                board.EnPassantSquareIndex = 0UL;

            if (isCapture || movingPiece == PieceType.Pawn)
                board.HalfMoves = 0;
            else
                board.HalfMoves++;
            board.Moves++;

            originalBoard = board;
            return true;
        }

        private static int GetPieceIndex( PieceType type, bool isWhite ) => (int)type + (isWhite ? 0 : 6);
    }
}
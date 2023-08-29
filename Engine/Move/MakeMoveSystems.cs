using Engine.Board;
using Engine.Data.Enums;
using Engine.Move;
using Engine.Zobrist;
using System.Runtime.CompilerServices;

namespace Engine.Board
{
    public ref partial struct BoardData
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MakeMove(MoveData move)
        {
            if (!MakeMoveSystems.MakeMove( ref this, move ))
                return false;
            MoveHistory.Add( _zobristKey );
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnmakeMove()
        {
            MoveHistory.RemoveLast();
        }
    }
}

namespace Engine.Move
{
    internal static class MakeMoveSystems
    {
        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        //makes pseudolegal move. (https://www.chessprogramming.org/Pseudo-Legal_Move) If move is not legal returns false and does not apply any changes to ref board.
        public static bool MakeMove(ref BoardData originalBoard, MoveData move )
        {
            BoardData board = originalBoard;

            //extracting data from MoveData struct
            int fromIndex = move.FromSquareIndex;
            int toIndex = move.ToSquareIndex;
            PieceType movingPiece = move.MovingPieceType;
            PieceType targetPiece = move.TargetPieceType;
            PieceType promotionPiece = move.PromotionPieceType;
            bool isCapture = move.IsCapture;
            bool isCastle = move.IsCastle;
            bool isEnPassant = move.IsEnPassant;
            bool isPromotion = move.IsPromotion;
            int sideToMove = board.SideToMove;
            bool isWhiteToMove = sideToMove == 0;

            //checks if move contains illegal castle, if so then we don't have to waste performance at rest of the code
            if (isCastle)
            {
                bool kingSide = toIndex - fromIndex == 2;
                if (!kingSide && board.IsSquareAttacked( isWhiteToMove ? 3 : 59, sideToMove ))
                    return false;
                else if (kingSide && board.IsSquareAttacked( isWhiteToMove ? 5 : 61, sideToMove ))
                    return false;
            }

            //removes piece from moving sqaure to then move it to its destination
            board.RemovePieceOnSquare( movingPiece, sideToMove, fromIndex );

            //handles captures & en passants
            if (isCapture)
            {
                if (isEnPassant)
                {
                    board.RemovePieceOnSquare( targetPiece, sideToMove ^ 1, toIndex + (isWhiteToMove ? -8 : 8) );
                } else
                {
                    board.RemovePieceOnSquare( targetPiece, sideToMove ^ 1, toIndex );
                }
            }

            //handles promotion
            if (!isPromotion)
            {
                board.SetPieceOnSquare( movingPiece, sideToMove, toIndex );
            } else
            {
                board.SetPieceOnSquare( promotionPiece, sideToMove, toIndex );
            }

            //if castle then moves rook as aditional rook to finish castle
            if (isCastle)
            {
                bool kingSide = toIndex - fromIndex == 2;
                if (kingSide)
                {
                    board.RemovePieceOnSquare( PieceType.Rook, sideToMove, toIndex + 1 );
                    board.SetPieceOnSquare( PieceType.Rook, sideToMove, toIndex - 1 );
                } else
                {
                    board.RemovePieceOnSquare( PieceType.Rook, sideToMove, toIndex - 2 );
                    board.SetPieceOnSquare( PieceType.Rook, sideToMove, toIndex + 1 );
                }
            }

            //checks if king is in check after move, and if so then move is illegal and returns false without applying changes
            bool isKingInCheck = board.IsSquareAttacked(board.GetPieceBitboard(5, sideToMove).LsbIndex, sideToMove );
            if (isKingInCheck)
            {
                return false;
            }

            //based on knowlage from check above, defines that current side king is not in check, but also stores if enemy king is in check to use that info later
            if (isWhiteToMove)
            {
                board.IsWhiteKingInCheck = 0;
                board.IsBlackKingInCheck = board.IsSquareAttacked( board.GetPieceBitboard( 5, 1 ).LsbIndex, 1 ) ? 1 : 0;
            } else
            {
                board.IsWhiteKingInCheck = board.IsSquareAttacked( board.GetPieceBitboard( 5, 0 ).LsbIndex, 0 ) ? 1 : 0;
                board.IsBlackKingInCheck = 0;
            }

            //swaps side to move
            board.SideToMove ^= 1;

            //updates castle rights if king moved
            if (movingPiece == PieceType.King)
            {
                if (isWhiteToMove)
                {
                    board.CanWhiteCastleKingSide = 0;
                    board.CanWhiteCastleQueenSide = 0;
                } else
                {
                    board.CanBlackCastleKingSide = 0;
                    board.CanBlackCastleQueenSide = 0;
                }
            }

            //updates castle rights if rook moved
            if (movingPiece == PieceType.Rook)
            {
                if (fromIndex == (isWhiteToMove ? 0 : 56))
                {
                    if (isWhiteToMove)
                        board.CanWhiteCastleQueenSide = 0;
                    else
                        board.CanBlackCastleQueenSide = 0;
                } else if (fromIndex == (isWhiteToMove ? 7 : 63))
                {
                    if (isWhiteToMove)
                        board.CanWhiteCastleKingSide = 0;
                    else
                        board.CanBlackCastleKingSide = 0;
                }
            }

            //updates castle rights if rook was captured
            if (targetPiece == PieceType.Rook)
            {
                if (toIndex == (!isWhiteToMove ? 0 : 56))
                {
                    if (!isWhiteToMove)
                        board.CanWhiteCastleQueenSide = 0;
                    else
                        board.CanBlackCastleQueenSide = 0;
                } else if (toIndex == (!isWhiteToMove ? 7 : 63))
                {
                    if (!isWhiteToMove)
                        board.CanWhiteCastleKingSide = 0;
                    else
                        board.CanBlackCastleKingSide = 0;
                }
            }

            //checks if pawn moves at least 2 squares and if so, puts en passant square behind him
            if (movingPiece == PieceType.Pawn && Math.Abs( fromIndex - toIndex ) == 16)
                board.EnPassantSquareIndex = fromIndex + (isWhiteToMove ? 8 : -8);
            else
                board.EnPassantSquareIndex = 0;

            //if there was a capture or pawn move then resets half moves counter, otherwise increase it
            if (isCapture || movingPiece == PieceType.Pawn)
            {
                board.HalfMoves = 0;
            } else
                board.HalfMoves++;
            board.Moves++;

            //updates zobrist key based on changes in the position
            board.ZobristKey = ZobristHashing.ModifyKey( board, originalBoard );
            originalBoard = board;
            return true;
        }
    }
}

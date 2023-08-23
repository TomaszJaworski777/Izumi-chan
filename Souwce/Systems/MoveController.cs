using System.Runtime.CompilerServices;
using Izumi.Helpers;
using Izumi.Structures;
using Izumi.Structures.Data;

namespace Izumi.Systems
{
    internal static class MoveController
    {
        private static ulong rank1 = 0x000000000000FF00;
        private static ulong rank7 = 0x00FF000000000000;
        private static ulong rank2 = 0x0000000000FF0000;
        private static ulong rank6 = 0x0000FF0000000000;

        private static ulong _kingSideCastleMask = 96;
        private static ulong _kingSideCastleReverserMask = 6917529027641081952;
        private static ulong _queenSideCastleMask = 14;
        private static ulong _queenSideCastleReverserMask = 1008806316530991118;

        private static Array64<Bitboard> _whitePawnMove = default;
        private static Array64<Bitboard> _blackPawnMove = default;

        static MoveController()
        {
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                if (squareIndex < 56)
                    _whitePawnMove[squareIndex].SetBitToOne( squareIndex + 8 );
                if (squareIndex > 7)
                    _blackPawnMove[squareIndex].SetBitToOne( squareIndex - 8 );
                if ((1UL << squareIndex & rank1) > 0)
                    _whitePawnMove[squareIndex].SetBitToOne( squareIndex + 16 );
                if ((1UL << squareIndex & rank7) > 0)
                    _blackPawnMove[squareIndex].SetBitToOne( squareIndex - 16 );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveOptimization )]
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

            if (isCastle)
            {
                bool kingSide = toIndex - fromIndex == 2;
                if (!kingSide && board.IsSquareAttacked( isWhiteToMove ? 3 : 59, isWhiteToMove ))
                    return false;
                else if (kingSide && board.IsSquareAttacked( isWhiteToMove ? 5 : 61, isWhiteToMove ))
                    return false;
            }

            board.RemovePieceOnSquare( movingPiece, isWhiteToMove, fromIndex );
            board.ZobristKey ^= ZobristHashing.Seeds[GetPieceIndex( movingPiece, isWhiteToMove ) * 64 + fromIndex];

            if (isCapture)
            {
                if (isEnPassant)
                {
                    board.RemovePieceOnSquare( targetPiece, !isWhiteToMove, toIndex + (isWhiteToMove ? -8 : 8) );
                    board.ZobristKey ^= ZobristHashing.Seeds[GetPieceIndex( targetPiece, !isWhiteToMove ) * 64 + toIndex + (isWhiteToMove ? -8 : 8)];
                } else
                {
                    board.RemovePieceOnSquare( targetPiece, !isWhiteToMove, toIndex );
                    board.ZobristKey ^= ZobristHashing.Seeds[GetPieceIndex( targetPiece, !isWhiteToMove ) * 64 + toIndex];
                }
            }

            if (!isPromotion)
            {
                board.SetPieceOnSquare( movingPiece, isWhiteToMove, toIndex );
                board.ZobristKey ^= ZobristHashing.Seeds[GetPieceIndex( movingPiece, isWhiteToMove ) * 64 + toIndex];
            } else
            {
                board.SetPieceOnSquare( promotionPiece, isWhiteToMove, toIndex );
                board.ZobristKey ^= ZobristHashing.Seeds[GetPieceIndex( promotionPiece, isWhiteToMove ) * 64 + toIndex];
            }

            if (isCastle)
            {
                bool kingSide = toIndex - fromIndex == 2;
                if (kingSide)
                {
                    board.RemovePieceOnSquare( PieceType.Rook, isWhiteToMove, toIndex + 1 );
                    board.SetPieceOnSquare( PieceType.Rook, isWhiteToMove, toIndex - 1 );
                    board.ZobristKey ^= ZobristHashing.Seeds[GetPieceIndex( PieceType.Rook, isWhiteToMove ) * 64 + toIndex + 1];
                    board.ZobristKey ^= ZobristHashing.Seeds[GetPieceIndex( PieceType.Rook, isWhiteToMove ) * 64 + (toIndex - 1)];
                } else
                {
                    board.RemovePieceOnSquare( PieceType.Rook, isWhiteToMove, toIndex - 2 );
                    board.SetPieceOnSquare( PieceType.Rook, isWhiteToMove, toIndex + 1 );
                    board.ZobristKey ^= ZobristHashing.Seeds[GetPieceIndex( PieceType.Rook, isWhiteToMove ) * 64 + (toIndex - 2)];
                    board.ZobristKey ^= ZobristHashing.Seeds[GetPieceIndex( PieceType.Rook, isWhiteToMove ) * 64 + toIndex + 1];
                }
            }

            if (board.IsKingInCheck( isWhiteToMove ))
            {
                return false;
            }

            if (isWhiteToMove)
            {
                board.WhiteKingInCheck = false;
                board.BlackKingInCheck = board.IsKingInCheck( false );
            } else
            {
                board.WhiteKingInCheck = board.IsKingInCheck( true );
                board.BlackKingInCheck = false;
            }

            board.IsWhiteToMove = !isWhiteToMove;

            if (movingPiece == PieceType.King)
            {
                if (isWhiteToMove)
                {
                    board.CanWhiteCastleKingSide = false;
                    board.CanWhiteCastleQueenSide = false;
                } else
                {
                    board.CanBlackCastleKingSide = false;
                    board.CanBlackCastleQueenSide = false;
                }
            }

            if (movingPiece == PieceType.Rook)
            {
                if (fromIndex == (isWhiteToMove ? 0 : 56))
                {
                    if (isWhiteToMove)
                        board.CanWhiteCastleQueenSide = false;
                    else
                        board.CanBlackCastleQueenSide = false;
                } else if (fromIndex == (isWhiteToMove ? 7 : 63))
                {
                    if (isWhiteToMove)
                        board.CanWhiteCastleKingSide = false;
                    else
                        board.CanBlackCastleKingSide = false;
                }
            }

            if (targetPiece == PieceType.Rook)
            {
                if (toIndex == (!isWhiteToMove ? 0 : 56))
                {
                    if (!isWhiteToMove)
                        board.CanWhiteCastleQueenSide = false;
                    else
                        board.CanBlackCastleQueenSide = false;
                } else if (toIndex == (!isWhiteToMove ? 7 : 63))
                {
                    if (!isWhiteToMove)
                        board.CanWhiteCastleKingSide = false;
                    else
                        board.CanBlackCastleKingSide = false;
                }
            }

            if (movingPiece == PieceType.Pawn && Math.Abs( fromIndex - toIndex ) == 16)
                board.EnPassantSquareIndex = fromIndex + (isWhiteToMove ? 8 : -8);
            else
                board.EnPassantSquareIndex = 0;

            if (isCapture || movingPiece == PieceType.Pawn)
            {
                board.HalfMoves = 0;
            } else
                board.HalfMoves++;
            board.Moves++;

            board.ZobristKey = ZobristHashing.ModifyKey( board, originalBoard );
            originalBoard = board;
            return true;
        }

        [MethodImpl( MethodImplOptions.AggressiveOptimization )]
        public static Span<Move> GeneratePseudoLegalMoves( Board board )
        {
            SpanMoveList moves = new();

            bool isWhiteToMove = board.IsWhiteToMove;
            Bitboard buffer, helperMask, attack;

            for (int pieceIndex = 0; pieceIndex < 6; pieceIndex++)
            {
                buffer = board.GetPieceBitboard(pieceIndex, isWhiteToMove);

                while (buffer > 0)
                {
                    int squareIndex = buffer.LsbIndex;
                    buffer &= buffer - 1;

                    if (pieceIndex == 0)
                    {
                        helperMask = isWhiteToMove ? _whitePawnMove[squareIndex] : _blackPawnMove[squareIndex];
                        if ((1UL << squareIndex & (isWhiteToMove ? rank1 : rank7)) > 0)
                        {
                            helperMask = new Bitboard( helperMask & ~board.AllPieces );
                            int maskedBitCount = helperMask.BitCount;
                            if (maskedBitCount == 2)
                            {
                                while (helperMask > 0)
                                {
                                    int lsbIndex = helperMask.LsbIndex;
                                    helperMask &= helperMask - 1;
                                    moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.None, PieceType.None, false, false, false, false ) );
                                }
                            } else if (maskedBitCount == 1)
                            {
                                int lsbIndex = helperMask.LsbIndex;
                                int delta = Math.Abs(squareIndex - lsbIndex);
                                if (delta == 8)
                                    moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.None, PieceType.None, false, false, false, false ) );
                            }
                        } else
                        {
                            helperMask = new Bitboard( helperMask & ~board.AllPieces );
                            if (helperMask > 0)
                            {
                                int lsbIndex = helperMask.LsbIndex;
                                if ((1UL << squareIndex & (isWhiteToMove ? rank7 : rank1)) > 0)
                                {
                                    moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.None, PieceType.Knight, false, false, false, true ) );
                                    moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.None, PieceType.Bishop, false, false, false, true ) );
                                    moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.None, PieceType.Rook, false, false, false, true ) );
                                    moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.None, PieceType.Queen, false, false, false, true ) );
                                } else
                                {
                                    moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.None, PieceType.None, false, false, false, false ) );
                                }
                            }
                        }

                        attack = isWhiteToMove ? PieceAttacks.WhitePawnAttackTable[squareIndex] : PieceAttacks.BlackPawnAttackTable[squareIndex];
                        helperMask = attack & board.OpponentPieces;
                        while (helperMask > 0)
                        {
                            int lsbIndex = helperMask.LsbIndex;
                            helperMask &= helperMask - 1;
                            PieceType findAttackedPiece = board.FindPieceTypeOnSquare(lsbIndex, !isWhiteToMove);
                            if ((1UL << squareIndex & (isWhiteToMove ? rank7 : rank1)) > 0)
                            {
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, findAttackedPiece, PieceType.Knight, false, true, false, true ) );
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, findAttackedPiece, PieceType.Bishop, false, true, false, true ) );
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, findAttackedPiece, PieceType.Rook, false, true, false, true ) );
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, findAttackedPiece, PieceType.Queen, false, true, false, true ) );
                            } else
                            {
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, findAttackedPiece, PieceType.None, false, true, false, false ) );
                            }
                        }
                        helperMask = attack & 1UL << board.EnPassantSquareIndex & (rank2 | rank6);
                        if (helperMask > 0)
                        {
                            int lsbIndex = helperMask.LsbIndex;
                            moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.Pawn, PieceType.None, false, true, true, false ) );
                        }

                        continue;
                    }

                    helperMask = board.OpponentPieces;

                    if (pieceIndex == 5)
                    {
                        if (!board.SideToMoveKingInCheck)
                        {
                            ulong mask = isWhiteToMove ? _queenSideCastleMask : _queenSideCastleMask ^ _queenSideCastleReverserMask;
                            if (board.CanCurrentSideCastleQueenSide && (board.AllPieces & mask) == 0)
                            {
                                moves.Add( new Move( squareIndex, isWhiteToMove ? 2 : 58, PieceType.King, PieceType.None, PieceType.None, true, false, false, false ) );
                            }

                            mask = isWhiteToMove ? _kingSideCastleMask : _kingSideCastleMask ^ _kingSideCastleReverserMask;
                            if (board.CanCurrentSideCastleKingSide && (board.AllPieces & mask) == 0)
                            {
                                moves.Add( new Move( squareIndex, isWhiteToMove ? 6 : 62, PieceType.King, PieceType.None, PieceType.None, true, false, false, false ) );
                            }
                        }

                        attack = PieceAttacks.KingAttacksTable[squareIndex] & ~board.SideToMovePieces;

                        while (attack > 0)
                        {
                            int lsbIndex = attack.LsbIndex;
                            attack &= attack - 1;
                            if (helperMask.GetBitValue( lsbIndex ) > 0)
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.King, board.FindPieceTypeOnSquare( lsbIndex, !isWhiteToMove ), PieceType.None, false, true, false, false ) );
                            else
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.King, PieceType.None, PieceType.None, false, false, false, false ) );
                        }

                        continue;
                    }

                    if (pieceIndex == 1)
                    {
                        attack = PieceAttacks.KnightAttackTable[squareIndex] & ~board.SideToMovePieces;

                        while (attack > 0)
                        {
                            int lsbIndex = attack.LsbIndex;
                            attack &= attack - 1;
                            if (helperMask.GetBitValue( lsbIndex ) > 0)
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Knight, board.FindPieceTypeOnSquare( lsbIndex, !isWhiteToMove ), PieceType.None, false, true, false, false ) );
                            else
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Knight, PieceType.None, PieceType.None, false, false, false, false ) );
                        }

                        continue;
                    }

                    if (pieceIndex == 2)
                    {
                        attack = PieceAttacks.GetBishopAttacks( squareIndex, board.AllPieces ) & ~board.SideToMovePieces;

                        while (attack > 0)
                        {
                            int lsbIndex = attack.LsbIndex;
                            attack &= attack - 1;
                            if (helperMask.GetBitValue( lsbIndex ) > 0)
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Bishop, board.FindPieceTypeOnSquare( lsbIndex, !isWhiteToMove ), PieceType.None, false, true, false, false ) );
                            else
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Bishop, PieceType.None, PieceType.None, false, false, false, false ) );
                        }

                        continue;
                    }

                    if (pieceIndex == 3)
                    {
                        attack = PieceAttacks.GetRookAttacks( squareIndex, board.AllPieces ) & ~board.SideToMovePieces;

                        while (attack > 0)
                        {
                            int lsbIndex = attack.LsbIndex;
                            attack &= attack - 1;
                            if (helperMask.GetBitValue( lsbIndex ) > 0)
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Rook, board.FindPieceTypeOnSquare( lsbIndex, !isWhiteToMove ), PieceType.None, false, true, false, false ) );
                            else
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Rook, PieceType.None, PieceType.None, false, false, false, false ) );
                        }

                        continue;
                    }

                    if (pieceIndex == 4)
                    {
                        attack = PieceAttacks.GetQueenAttacks( squareIndex, board.AllPieces ) & ~board.SideToMovePieces;

                        while (attack > 0)
                        {
                            int lsbIndex = attack.LsbIndex;
                            attack &= attack - 1;
                            if (helperMask.GetBitValue( lsbIndex ) > 0)
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Queen, board.FindPieceTypeOnSquare( lsbIndex, !isWhiteToMove ), PieceType.None, false, true, false, false ) );
                            else
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Queen, PieceType.None, PieceType.None, false, false, false, false ) );
                        }

                        continue;
                    }
                }
            }

            return moves.Values;
        }

        [MethodImpl( MethodImplOptions.AggressiveOptimization )]
        public static Span<Move> GeneratePseudoLegalPriorityMoves( Board board )
        {
            SpanMoveList moves = new();

            bool isWhiteToMove = board.IsWhiteToMove;
            Bitboard buffer, helperMask, attack;

            for (int pieceIndex = 0; pieceIndex < 6; pieceIndex++)
            {
                buffer = board.GetPieceBitboard( pieceIndex, isWhiteToMove );

                while (buffer > 0)
                {
                    int squareIndex = buffer.LsbIndex;
                    buffer &= buffer - 1;

                    if (pieceIndex == 0)
                    {
                        helperMask = isWhiteToMove ? _whitePawnMove[squareIndex] : _blackPawnMove[squareIndex];
                        helperMask = new Bitboard( helperMask & ~board.AllPieces );
                        if (helperMask > 0)
                        {
                            int lsbIndex = helperMask.LsbIndex;
                            if ((1UL << squareIndex & (isWhiteToMove ? rank7 : rank1)) > 0)
                            {
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.None, PieceType.Knight, false, false, false, true ) );
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.None, PieceType.Bishop, false, false, false, true ) );
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.None, PieceType.Rook, false, false, false, true ) );
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.None, PieceType.Queen, false, false, false, true ) );
                            }
                        }

                        attack = isWhiteToMove ? PieceAttacks.WhitePawnAttackTable[squareIndex] : PieceAttacks.BlackPawnAttackTable[squareIndex];
                        helperMask = attack & board.OpponentPieces;
                        while (helperMask > 0)
                        {
                            int lsbIndex = helperMask.LsbIndex;
                            helperMask &= helperMask - 1;
                            PieceType findAttackedPiece = board.FindPieceTypeOnSquare(lsbIndex, !isWhiteToMove);
                            if ((1UL << squareIndex & (isWhiteToMove ? rank7 : rank1)) > 0)
                            {
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, findAttackedPiece, PieceType.Knight, false, true, false, true ) );
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, findAttackedPiece, PieceType.Bishop, false, true, false, true ) );
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, findAttackedPiece, PieceType.Rook, false, true, false, true ) );
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, findAttackedPiece, PieceType.Queen, false, true, false, true ) );
                            } else
                            {
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, findAttackedPiece, PieceType.None, false, true, false, false ) );
                            }
                        }
                        helperMask = attack & 1UL << (int)board.EnPassantSquareIndex & (rank2 | rank6);
                        if (helperMask > 0)
                        {
                            int lsbIndex = helperMask.LsbIndex;
                            moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.Pawn, PieceType.None, false, true, true, false ) );
                        }

                        continue;
                    }

                    helperMask = board.OpponentPieces;

                    if (pieceIndex == 5)
                    {
                        if (!board.SideToMoveKingInCheck)
                        {
                            ulong mask = isWhiteToMove ? _queenSideCastleMask : _queenSideCastleMask ^ _queenSideCastleReverserMask;
                            if (board.CanCurrentSideCastleQueenSide && (board.AllPieces & mask) == 0)
                            {
                                moves.Add( new Move( squareIndex, isWhiteToMove ? 2 : 58, PieceType.King, PieceType.None, PieceType.None, true, false, false, false ) );
                            }

                            mask = isWhiteToMove ? _kingSideCastleMask : _kingSideCastleMask ^ _kingSideCastleReverserMask;
                            if (board.CanCurrentSideCastleKingSide && (board.AllPieces & mask) == 0)
                            {
                                moves.Add( new Move( squareIndex, isWhiteToMove ? 6 : 62, PieceType.King, PieceType.None, PieceType.None, true, false, false, false ) );
                            }
                        }

                        attack = PieceAttacks.KingAttacksTable[squareIndex] & ~board.SideToMovePieces;

                        while (attack > 0)
                        {
                            int lsbIndex = attack.LsbIndex;
                            attack &= attack - 1;
                            if (helperMask.GetBitValue( lsbIndex ) > 0)
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.King, board.FindPieceTypeOnSquare( lsbIndex, !isWhiteToMove ), PieceType.None, false, true, false, false ) );
                        }

                        continue;
                    }

                    if (pieceIndex == 1)
                    {
                        attack = PieceAttacks.KnightAttackTable[squareIndex] & ~board.SideToMovePieces;

                        while (attack > 0)
                        {
                            int lsbIndex = attack.LsbIndex;
                            attack &= attack - 1;
                            if (helperMask.GetBitValue( lsbIndex ) > 0)
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Knight, board.FindPieceTypeOnSquare( lsbIndex, !isWhiteToMove ), PieceType.None, false, true, false, false ) );
                        }

                        continue;
                    }

                    if (pieceIndex == 2)
                    {
                        attack = PieceAttacks.GetBishopAttacks( squareIndex, board.AllPieces ) & ~board.SideToMovePieces;

                        while (attack > 0)
                        {
                            int lsbIndex = attack.LsbIndex;
                            attack &= attack - 1;
                            if (helperMask.GetBitValue( lsbIndex ) > 0)
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Bishop, board.FindPieceTypeOnSquare( lsbIndex, !isWhiteToMove ), PieceType.None, false, true, false, false ) );
                        }

                        continue;
                    }

                    if (pieceIndex == 3)
                    {
                        attack = PieceAttacks.GetRookAttacks( squareIndex, board.AllPieces ) & ~board.SideToMovePieces;

                        while (attack > 0)
                        {
                            int lsbIndex = attack.LsbIndex;
                            attack &= attack - 1;
                            if (helperMask.GetBitValue( lsbIndex ) > 0)
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Rook, board.FindPieceTypeOnSquare( lsbIndex, !isWhiteToMove ), PieceType.None, false, true, false, false ) );
                        }

                        continue;
                    }

                    if (pieceIndex == 4)
                    {
                        attack = PieceAttacks.GetQueenAttacks( squareIndex, board.AllPieces ) & ~board.SideToMovePieces;

                        while (attack > 0)
                        {
                            int lsbIndex = attack.LsbIndex;
                            attack &= attack - 1;
                            if (helperMask.GetBitValue( lsbIndex ) > 0)
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Queen, board.FindPieceTypeOnSquare( lsbIndex, !isWhiteToMove ), PieceType.None, false, true, false, false ) );
                        }

                        continue;
                    }
                }
            }

            return moves.Values;
        }

        private static int GetPieceIndex( PieceType type, bool isWhite ) => (int)type + (isWhite ? 0 : 6);

        private unsafe ref struct SpanMoveList
        {
            private Span<Move> _values;
            private int _moveCount;

            public SpanMoveList()
            {
                _values = new Move[300];
                _moveCount = 0;
            }

            internal readonly Span<Move> Values => _values[.._moveCount];

            public void Add( Move move )
            {
                _values[_moveCount] = move;
                _moveCount++;
            }
        }
    }
}
using System.Runtime.CompilerServices;

namespace Greg
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
                if (((1UL << squareIndex) & rank1) > 0)
                    _whitePawnMove[squareIndex].SetBitToOne( squareIndex + 16 );
                if (((1UL << squareIndex) & rank7) > 0)
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

            board.Data[currentSideAllPieceIndex].SetBitToZero( fromIndex );
            board.Data[currentSideAllPieceIndex].SetBitToOne( toIndex );
            board.Data[14] = board.Data[12] | board.Data[13];

            if (board.IsKingInCheck( isWhiteToMove ))
            {
                return false;
            }

            board.IsWhiteToMove = !isWhiteToMove;

            if (movingPiece == PieceType.King)
            {
                board.Data[16].SetBitToZero( isWhiteToMove ? 0 : 2 );
                board.Data[16].SetBitToZero( isWhiteToMove ? 1 : 3 );
            }

            if (movingPiece == PieceType.Rook)
            {
                if (fromIndex == (isWhiteToMove ? 0 : 56))
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

        [MethodImpl( MethodImplOptions.AggressiveOptimization )]
        public static Span<Move> GeneratePseudoLegalMoves( Board board )
        {
            SpanMoveList moves = new();

            bool isWhiteToMove = board.IsWhiteToMove;
            Bitboard buffer, helperMask, attack;

            for (int pieceIndex = 0; pieceIndex < 6; pieceIndex++)
            {
                buffer = board.Data[pieceIndex + (isWhiteToMove ? 0 : 6)];

                while (buffer > 0)
                {
                    int squareIndex = buffer.LsbIndex;
                    buffer &= buffer - 1;

                    if (pieceIndex == 0)
                    {
                        helperMask = isWhiteToMove ? _whitePawnMove[squareIndex] : _blackPawnMove[squareIndex];
                        if (((1UL << squareIndex) & (isWhiteToMove ? rank1 : rank7)) > 0)
                        {
                            helperMask = new Bitboard( helperMask & ~board.Data[14] );
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
                            helperMask = new Bitboard( helperMask & ~board.Data[14] );
                            if (helperMask > 0)
                            {
                                int lsbIndex = helperMask.LsbIndex;
                                if (((1UL << squareIndex) & (isWhiteToMove ? rank7 : rank1)) > 0)
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
                        helperMask = attack & (isWhiteToMove ? board.Data[13] : board.Data[12]);
                        while (helperMask > 0)
                        {
                            int lsbIndex = helperMask.LsbIndex;
                            helperMask &= helperMask - 1;
                            PieceType findAttackedPiece = board.FindPieceTypeOnSquare( lsbIndex, !isWhiteToMove);
                            if (((1UL << squareIndex) & (isWhiteToMove ? rank7 : rank1)) > 0)
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
                        helperMask = attack & (1UL << (int)board.EnPassantSquareIndex) & (rank2 | rank6);
                        if (helperMask > 0)
                        {
                            int lsbIndex = helperMask.LsbIndex;
                            moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.Pawn, PieceType.None, false, true, true, false ) );
                        }

                        continue;
                    }

                    helperMask = isWhiteToMove ? board.Data[13] : board.Data[12];

                    if (pieceIndex == 5)
                    {
                        if(!board.IsKingInCheck(isWhiteToMove))
                        {
                            ulong mask = isWhiteToMove ? _queenSideCastleMask : (_queenSideCastleMask ^ _queenSideCastleReverserMask);
                            if (board.Data[16].GetBitValue( isWhiteToMove ? 0 : 2 ) > 0 && (board.Data[14] & mask) == 0 &&
                                !board.IsSquareAttacked( isWhiteToMove ? 2 : 58, isWhiteToMove ) && !board.IsSquareAttacked( isWhiteToMove ? 3 : 59, isWhiteToMove ))
                            {
                                moves.Add( new Move( squareIndex, isWhiteToMove ? 2 : 58, PieceType.King, PieceType.None, PieceType.None, true, false, false, false ) );
                            }

                            mask = isWhiteToMove ? _kingSideCastleMask : (_kingSideCastleMask ^ _kingSideCastleReverserMask);
                            if (board.Data[16].GetBitValue( isWhiteToMove ? 1 : 3 ) > 0 && (board.Data[14] & mask) == 0 &&
                                !board.IsSquareAttacked( isWhiteToMove ? 5 : 61, isWhiteToMove ) && !board.IsSquareAttacked( isWhiteToMove ? 6 : 62, isWhiteToMove ))
                            {
                                moves.Add( new Move( squareIndex, isWhiteToMove ? 6 : 62, PieceType.King, PieceType.None, PieceType.None, true, false, false, false ) );
                            }
                        }

                        attack = PieceAttacks.KingAttacksTable[squareIndex] & ~(isWhiteToMove ? board.Data[12] : board.Data[13]);

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

                    if ( pieceIndex == 1)
                    {
                        attack = PieceAttacks.KnightAttackTable[squareIndex] & ~(isWhiteToMove ? board.Data[12] : board.Data[13]);

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
                        attack = PieceAttacks.GetBishopAttacks( squareIndex, board.Data[14] ) & ~(isWhiteToMove ? board.Data[12] : board.Data[13]);

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
                        attack = PieceAttacks.GetRookAttacks( squareIndex, board.Data[14] ) & ~(isWhiteToMove ? board.Data[12] : board.Data[13]);

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
                        attack = PieceAttacks.GetQueenAttacks( squareIndex, board.Data[14] ) & ~(isWhiteToMove ? board.Data[12] : board.Data[13]);

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
                buffer = board.Data[pieceIndex + (isWhiteToMove ? 0 : 6)];

                while (buffer > 0)
                {
                    int squareIndex = buffer.LsbIndex;
                    buffer &= buffer - 1;

                    if (pieceIndex == 0)
                    {
                        helperMask = isWhiteToMove ? _whitePawnMove[squareIndex] : _blackPawnMove[squareIndex];
                        helperMask = new Bitboard( helperMask & ~board.Data[14] );
                        if (helperMask > 0)
                        {
                            int lsbIndex = helperMask.LsbIndex;
                            if (((1UL << squareIndex) & (isWhiteToMove ? rank7 : rank1)) > 0)
                            {
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.None, PieceType.Knight, false, false, false, true ) );
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.None, PieceType.Bishop, false, false, false, true ) );
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.None, PieceType.Rook, false, false, false, true ) );
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.None, PieceType.Queen, false, false, false, true ) );
                            }
                        }

                        attack = isWhiteToMove ? PieceAttacks.WhitePawnAttackTable[squareIndex] : PieceAttacks.BlackPawnAttackTable[squareIndex];
                        helperMask = attack & (isWhiteToMove ? board.Data[13] : board.Data[12]);
                        while (helperMask > 0)
                        {
                            int lsbIndex = helperMask.LsbIndex;
                            helperMask &= helperMask - 1;
                            PieceType findAttackedPiece = board.FindPieceTypeOnSquare( lsbIndex, !isWhiteToMove);
                            if (((1UL << squareIndex) & (isWhiteToMove ? rank7 : rank1)) > 0)
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
                        helperMask = attack & (1UL << (int)board.EnPassantSquareIndex) & (rank2 | rank6);
                        if (helperMask > 0)
                        {
                            int lsbIndex = helperMask.LsbIndex;
                            moves.Add( new Move( squareIndex, lsbIndex, PieceType.Pawn, PieceType.Pawn, PieceType.None, false, true, true, false ) );
                        }

                        continue;
                    }

                    helperMask = isWhiteToMove ? board.Data[13] : board.Data[12];

                    if (pieceIndex == 5)
                    {
                        if(!board.IsKingInCheck( isWhiteToMove ))
                        {
                            ulong mask = isWhiteToMove ? _queenSideCastleMask : (_queenSideCastleMask ^ _queenSideCastleReverserMask);
                            if (board.Data[16].GetBitValue( isWhiteToMove ? 0 : 2 ) > 0 && (board.Data[14] & mask) == 0 &&
                                !board.IsSquareAttacked( isWhiteToMove ? 2 : 58, isWhiteToMove ) && !board.IsSquareAttacked( isWhiteToMove ? 3 : 59, isWhiteToMove ))
                            {
                                moves.Add( new Move( squareIndex, isWhiteToMove ? 2 : 58, PieceType.King, PieceType.None, PieceType.None, true, false, false, false ) );
                            }

                            mask = isWhiteToMove ? _kingSideCastleMask : (_kingSideCastleMask ^ _kingSideCastleReverserMask);
                            if (board.Data[16].GetBitValue( isWhiteToMove ? 1 : 3 ) > 0 && (board.Data[14] & mask) == 0 &&
                                !board.IsSquareAttacked( isWhiteToMove ? 5 : 61, isWhiteToMove ) && !board.IsSquareAttacked( isWhiteToMove ? 6 : 62, isWhiteToMove ))
                            {
                                moves.Add( new Move( squareIndex, isWhiteToMove ? 6 : 62, PieceType.King, PieceType.None, PieceType.None, true, false, false, false ) );
                            }
                        }

                        attack = PieceAttacks.KingAttacksTable[squareIndex] & ~(isWhiteToMove ? board.Data[12] : board.Data[13]);

                        while (attack > 0)
                        {
                            int lsbIndex = attack.LsbIndex;
                            attack &= attack - 1;
                            if (helperMask.GetBitValue( lsbIndex ) > 0)
                                moves.Add( new Move( squareIndex, lsbIndex, PieceType.King, board.FindPieceTypeOnSquare( lsbIndex, !isWhiteToMove ), PieceType.None, false, true, false, false ) );
                        }

                        continue;
                    }

                    if ( pieceIndex == 1)
                    {
                        attack = PieceAttacks.KnightAttackTable[squareIndex] & ~(isWhiteToMove ? board.Data[12] : board.Data[13]);

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
                        attack = PieceAttacks.GetBishopAttacks( squareIndex, board.Data[14] ) & ~(isWhiteToMove ? board.Data[12] : board.Data[13]);

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
                        attack = PieceAttacks.GetRookAttacks( squareIndex, board.Data[14] ) & ~(isWhiteToMove ? board.Data[12] : board.Data[13]);

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
                        attack = PieceAttacks.GetQueenAttacks( squareIndex, board.Data[14] ) & ~(isWhiteToMove ? board.Data[12] : board.Data[13]);

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
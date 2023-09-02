using System;
using System.Runtime.CompilerServices;
using Engine.Board;
using Engine.Data.Bitboards;
using Engine.Data.Enums;
using Engine.Move;
using Engine.PieceAttacks;
using Engine.Utils;

namespace Engine.Board
{
    public partial struct BoardData
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void GenerateAllPseudoLegalMoves( ref MoveList moveList ) => MoveGenerator.GenerateAllPseudoLegalMoves( ref this, ref moveList );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void GenerateTacticalPseudoLegalMoves( ref MoveList moveList ) => MoveGenerator.GenerateTacticalPseudoLegalMoves( ref this, ref moveList );
    }
}

namespace Engine.Move
{
    internal static class MoveGenerator
    {
        private const ulong Rank1 = 0x000000000000FF00;
        private const ulong Rank7 = 0x00FF000000000000;
        private const ulong Rank2 = 0x0000000000FF0000;
        private const ulong Rank6 = 0x0000FF0000000000;

        private const ulong KingSideCastleMask = 96;
        private const ulong KingSideCastleReverserMask = 6917529027641081952;
        private const ulong QueenSideCastleMask = 14;
        private const ulong QueenSideCastleReverserMask = 1008806316530991118;

        private static Array64<Bitboard> _whitePawnMove;
        private static Array64<Bitboard> _blackPawnMove;

        //generates helper boards to increase performance of move generation
        static MoveGenerator()
        {
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                if (squareIndex < 56)
                    _whitePawnMove[squareIndex].SetBitToOne( squareIndex + 8 );
                if (squareIndex > 7)
                    _blackPawnMove[squareIndex].SetBitToOne( squareIndex - 8 );
                if ((1UL << squareIndex & Rank1) > 0)
                    _whitePawnMove[squareIndex].SetBitToOne( squareIndex + 16 );
                if ((1UL << squareIndex & Rank7) > 0)
                    _blackPawnMove[squareIndex].SetBitToOne( squareIndex - 16 );
            }
        }

        //generates all moves without checking if they will leave king in check. (https://www.chessprogramming.org/Pseudo-Legal_Move)
        public static void GenerateAllPseudoLegalMoves( ref BoardData board, ref MoveList moves )
        {
            //prepares some data to save performance accessing it later
            bool isWhiteToMove = board.SideToMove == 0;
            Array64<Bitboard> pawnMove;
            ulong rank;
            ulong rankReverse;
            if (isWhiteToMove)
            {
                pawnMove = _whitePawnMove;
                rank = Rank1;
                rankReverse = Rank7;
            }
            else
            {
                pawnMove = _blackPawnMove;
                rank = Rank7;
                rankReverse = Rank1;
            }

            int sideToMove = board.SideToMove;

            Bitboard allPieces = board.GetPiecesBitboardForSide( 0 ) | board.GetPiecesBitboardForSide( 1 ), 
                opponentPieces = board.GetPiecesBitboardForSide( sideToMove ^ 1 ),
                alliedPiecesInvertMask = ~board.GetPiecesBitboardForSide(sideToMove);

            //iterates through every piece
            for (int pieceIndex = 0; pieceIndex < 6; pieceIndex++)
            {
                Bitboard buffer = board.GetPieceBitboard( pieceIndex, sideToMove );

                //adds moves to every instance of this piece of the board
                while (buffer > 0)
                {
                    int squareIndex = buffer.LsbIndex;
                    buffer &= buffer - 1;

                    //handle pawns
                    Bitboard helperMask = opponentPieces;
                    Bitboard attack;

                    switch (pieceIndex)
                    {
                        case 0:
                        {
                            //moving pawn forward
                            helperMask = pawnMove[squareIndex];
                            if ((1UL << squareIndex & rank) > 0)
                            {
                                helperMask &= ~allPieces;
                                int maskedBitCount = helperMask.BitCount;
                                switch (maskedBitCount)
                                {
                                    case 2:
                                    {
                                        while (helperMask > 0)
                                        {
                                            int lsbIndex = helperMask.LsbIndex;
                                            helperMask &= helperMask - 1;
                                            moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Pawn, false, false, false, false ) );
                                        }

                                        break;
                                    }
                                    case 1:
                                    {
                                        int lsbIndex = helperMask.LsbIndex;
                                        int delta = Math.Abs(squareIndex - lsbIndex);
                                        if (delta == 8)
                                        {
                                            moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.None, false, false, false, false ) );
                                        }

                                        break;
                                    }
                                }
                            } else
                            {
                                helperMask &= ~allPieces;
                                if (helperMask > 0)
                                {
                                    int lsbIndex = helperMask.LsbIndex;
                                    if ((1UL << squareIndex & rankReverse) > 0)
                                    {
                                        moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Knight, false, false, false, true ) );
                                        moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Bishop, false, false, false, true ) );
                                        moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Rook, false, false, false, true ) );
                                        moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Queen, false, false, false, true ) );
                                    } else
                                    {
                                        moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.None, false, false, false, false ) );
                                    }
                                }
                            }

                            //captures
                            attack = PieceAttackProvider.GetPawnAttacks( squareIndex, sideToMove );
                            helperMask = attack & opponentPieces;
                            while (helperMask > 0)
                            {
                                int lsbIndex = helperMask.LsbIndex;
                                helperMask &= helperMask - 1;
                                if ((1UL << squareIndex & rankReverse) > 0)
                                {
                                    moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Knight, true, false, false, true ) );
                                    moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Bishop, true, false, false, true ) );
                                    moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Rook, true, false, false, true ) );
                                    moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Queen, true, false, false, true ) );
                                } else
                                {
                                    moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.None, true, false, false, false ) );
                                }
                            }
                            helperMask = attack & 1UL << board.EnPassantSquareIndex & (Rank2 | Rank6);
                            if (helperMask > 0)
                            {
                                int lsbIndex = helperMask.LsbIndex;
                                moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.None, true, false, true, false ) );
                            }

                            continue;
                        }
                        case 5:
                        {
                            //castle moves
                            bool canCurrentSideCastleQueenSide = (isWhiteToMove ? board.CanWhiteCastleQueenSide : board.CanBlackCastleQueenSide) > 0;
                            bool canCurrentSideCastleKingSide = (isWhiteToMove ? board.CanWhiteCastleKingSide : board.CanBlackCastleKingSide) > 0;

                            if ((isWhiteToMove ? board.IsWhiteKingInCheck : board.IsBlackKingInCheck) == 0)
                            {
                                ulong mask = isWhiteToMove ? QueenSideCastleMask : QueenSideCastleMask ^ QueenSideCastleReverserMask;

                                if (canCurrentSideCastleQueenSide && (allPieces & mask) == 0)
                                {
                                    moves.Add(new MoveData(board, squareIndex, isWhiteToMove ? 2 : 58, PieceType.None, false, true, false, false));
                                }

                                mask = isWhiteToMove ? KingSideCastleMask : KingSideCastleMask ^ KingSideCastleReverserMask;

                                if (canCurrentSideCastleKingSide && (allPieces & mask) == 0)
                                {
                                    moves.Add(new MoveData(board, squareIndex, isWhiteToMove ? 6 : 62, PieceType.None, false, true, false, false));
                                }
                            }

                            //moves/captures
                            attack = PieceAttackProvider.GetKingAttacks(squareIndex) & alliedPiecesInvertMask;

                            while (attack > 0)
                            {
                                int lsbIndex = attack.LsbIndex;
                                attack &= attack - 1;
                                moves.Add(helperMask.GetBitValue(lsbIndex) > 0 ? new MoveData(board, squareIndex, lsbIndex, PieceType.None, true, false, false, false) : new MoveData(board, squareIndex, lsbIndex, PieceType.None, false, false, false, false));
                            }

                            continue;
                        }
                        //handles knights' moves/captures
                        case 1:
                        {
                            attack = PieceAttackProvider.GetKnightAttacks(squareIndex) & alliedPiecesInvertMask;

                            while (attack > 0)
                            {
                                int lsbIndex = attack.LsbIndex;
                                attack &= attack - 1;
                                moves.Add(helperMask.GetBitValue(lsbIndex) > 0 ? new MoveData(board, squareIndex, lsbIndex, PieceType.None, true, false, false, false) : new MoveData(board, squareIndex, lsbIndex, PieceType.None, false, false, false, false));
                            }

                            continue;
                        }
                        //handles bishops' moves/captures
                        case 2:
                        {
                            attack = PieceAttackProvider.GetBishopAttacks(squareIndex, allPieces) & alliedPiecesInvertMask;

                            while (attack > 0)
                            {
                                int lsbIndex = attack.LsbIndex;
                                attack &= attack - 1;
                                moves.Add(helperMask.GetBitValue(lsbIndex) > 0 ? new MoveData(board, squareIndex, lsbIndex, PieceType.None, true, false, false, false) : new MoveData(board, squareIndex, lsbIndex, PieceType.None, false, false, false, false));
                            }

                            continue;
                        }
                        //handles rooks' moves/captures
                        case 3:
                        {
                            attack = PieceAttackProvider.GetRookAttacks(squareIndex, allPieces) & alliedPiecesInvertMask;

                            while (attack > 0)
                            {
                                int lsbIndex = attack.LsbIndex;
                                attack &= attack - 1;
                                moves.Add(helperMask.GetBitValue(lsbIndex) > 0 ? new MoveData(board, squareIndex, lsbIndex, PieceType.None, true, false, false, false) : new MoveData(board, squareIndex, lsbIndex, PieceType.None, false, false, false, false));
                            }

                            continue;
                        }
                        //handles queens' moves/captures
                        case 4:
                        {
                            attack = PieceAttackProvider.GetQueenAttacks(squareIndex, allPieces) & alliedPiecesInvertMask;

                            while (attack > 0)
                            {
                                int lsbIndex = attack.LsbIndex;
                                attack &= attack - 1;
                                moves.Add(helperMask.GetBitValue(lsbIndex) > 0 ? new MoveData(board, squareIndex, lsbIndex, PieceType.None, true, false, false, false) : new MoveData(board, squareIndex, lsbIndex, PieceType.None, false, false, false, false));
                            }

                            break;
                        }
                    }
                }
            }
        }

        //generates only tactical moves (https://www.chessprogramming.org/Tactical_Moves) without checking if they will leave king in check.
        //(https://www.chessprogramming.org/Pseudo-Legal_Move)
        public static void GenerateTacticalPseudoLegalMoves( ref BoardData board, ref MoveList moves )
        {
            //prepares some data to save performance accessing it later
            Array64<Bitboard> pawnMove;
            ulong rank;
            ulong rankReverse;

            if (board.SideToMove == 0)
            {
                pawnMove = _whitePawnMove;
                rank = Rank1;
                rankReverse = Rank7;
            }
            else
            {
                pawnMove = _blackPawnMove;
                rank = Rank7;
                rankReverse = Rank1;
            }

            int sideToMove = board.SideToMove;

            Bitboard allPieces = board.GetPiecesBitboardForSide( 0 ) | board.GetPiecesBitboardForSide( 1 ),
                opponentPieces = board.GetPiecesBitboardForSide( sideToMove ^ 1 ),
                alliedPiecesInvertMask = ~board.GetPiecesBitboardForSide(sideToMove);

            //iterates through every piece
            for (int pieceIndex = 0; pieceIndex < 6; pieceIndex++)
            {
                Bitboard buffer = board.GetPieceBitboard( pieceIndex, sideToMove );

                //adds moves to every instance of this piece of the board
                while (buffer > 0)
                {
                    int squareIndex = buffer.LsbIndex;
                    buffer &= buffer - 1;

                    //handle pawns
                    Bitboard helperMask = opponentPieces;
                    Bitboard attack;

                    switch (pieceIndex)
                    {
                        case 0:
                        {
                            //pawn promotion
                            if ((1UL << squareIndex & rank) == 0)
                            {
                                helperMask = pawnMove[squareIndex] & ~allPieces;
                                if (helperMask > 0)
                                {
                                    int lsbIndex = helperMask.LsbIndex;
                                    if ((1UL << squareIndex & rankReverse) > 0)
                                    {
                                        moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Knight, false, false, false, true ) );
                                        moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Bishop, false, false, false, true ) );
                                        moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Rook, false, false, false, true ) );
                                        moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Queen, false, false, false, true ) );
                                    }
                                }
                            }

                            //captures
                            attack = PieceAttackProvider.GetPawnAttacks( squareIndex, sideToMove );
                            helperMask = attack & opponentPieces;
                            while (helperMask > 0)
                            {
                                int lsbIndex = helperMask.LsbIndex;
                                helperMask &= helperMask - 1;
                                if ((1UL << squareIndex & rankReverse) > 0)
                                {
                                    moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Knight, true, false, false, true ) );
                                    moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Bishop, true, false, false, true ) );
                                    moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Rook, true, false, false, true ) );
                                    moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.Queen, true, false, false, true ) );
                                } else
                                {
                                    moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.None, true, false, false, false ) );
                                }
                            }
                            helperMask = attack & 1UL << board.EnPassantSquareIndex & (Rank2 | Rank6);
                            if (helperMask > 0)
                            {
                                int lsbIndex = helperMask.LsbIndex;
                                moves.Add( new MoveData( board, squareIndex, lsbIndex, PieceType.None, true, false, true, false ) );
                            }

                            continue;
                        }
                        case 5:
                        {
                            //captures
                            attack = PieceAttackProvider.GetKingAttacks(squareIndex) & alliedPiecesInvertMask;

                            while (attack > 0)
                            {
                                int lsbIndex = attack.LsbIndex;
                                attack &= attack - 1;

                                if (helperMask.GetBitValue(lsbIndex) > 0)
                                    moves.Add(new MoveData(board, squareIndex, lsbIndex, PieceType.None, true, false, false, false));
                            }

                            continue;
                        }
                        //handles knights' captures
                        case 1:
                        {
                            attack = PieceAttackProvider.GetKnightAttacks(squareIndex) & alliedPiecesInvertMask;

                            while (attack > 0)
                            {
                                int lsbIndex = attack.LsbIndex;
                                attack &= attack - 1;

                                if (helperMask.GetBitValue(lsbIndex) > 0)
                                    moves.Add(new MoveData(board, squareIndex, lsbIndex, PieceType.None, true, false, false, false));
                            }

                            continue;
                        }
                        //handles bishops' captures
                        case 2:
                        {
                            attack = PieceAttackProvider.GetBishopAttacks(squareIndex, allPieces) & alliedPiecesInvertMask;

                            while (attack > 0)
                            {
                                int lsbIndex = attack.LsbIndex;
                                attack &= attack - 1;

                                if (helperMask.GetBitValue(lsbIndex) > 0)
                                    moves.Add(new MoveData(board, squareIndex, lsbIndex, PieceType.None, true, false, false, false));
                            }

                            continue;
                        }
                        //handles rooks' captures
                        case 3:
                        {
                            attack = PieceAttackProvider.GetRookAttacks(squareIndex, allPieces) & alliedPiecesInvertMask;

                            while (attack > 0)
                            {
                                int lsbIndex = attack.LsbIndex;
                                attack &= attack - 1;

                                if (helperMask.GetBitValue(lsbIndex) > 0)
                                    moves.Add(new MoveData(board, squareIndex, lsbIndex, PieceType.None, true, false, false, false));
                            }

                            continue;
                        }
                        //handles queens' captures
                        case 4:
                        {
                            attack = PieceAttackProvider.GetQueenAttacks(squareIndex, allPieces) & alliedPiecesInvertMask;

                            while (attack > 0)
                            {
                                int lsbIndex = attack.LsbIndex;
                                attack &= attack - 1;

                                if (helperMask.GetBitValue(lsbIndex) > 0)
                                    moves.Add(new MoveData(board, squareIndex, lsbIndex, PieceType.None, true, false, false, false));
                            }

                            break;
                        }
                    }
                }
            }
        }
    }
}

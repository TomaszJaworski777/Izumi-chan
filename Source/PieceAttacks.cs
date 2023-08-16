using System.Runtime.CompilerServices;

namespace Izumi
{
    internal static class PieceAttacks
    {
        public static Array64<Bitboard> WhitePawnAttackTable = default;
        public static Array64<Bitboard> BlackPawnAttackTable = default;
        public static Array64<Bitboard> KnightAttackTable = default;
        public static Array64<Bitboard> KingAttacksTable = default;

        private static SlidingPieceAttacks? _slidingPieceAttacks;

        public static void Initizalize()
        {
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                InitializePawnAttacksForIndex( squareIndex );
                InitializeKnightAttacksForIndex( squareIndex );
                InitializeKingAttacksForIndex( squareIndex );
            }

            _slidingPieceAttacks = new();
        }

        private static void InitializePawnAttacksForIndex( int squareIndex )
        {
            Square square = new Square(squareIndex);

            if (square.File > 0 && square.Rank < 7)
                WhitePawnAttackTable[squareIndex].SetBitToOne( squareIndex + 7 );
            if (square.File < 7 && square.Rank < 7)
                WhitePawnAttackTable[squareIndex].SetBitToOne( squareIndex + 9 );
            if (square.File < 7 && square.Rank > 0)
                BlackPawnAttackTable[squareIndex].SetBitToOne( squareIndex - 7 );
            if (square.File > 0 && square.Rank > 0)
                BlackPawnAttackTable[squareIndex].SetBitToOne( squareIndex - 9 );
        }

        private static void InitializeKnightAttacksForIndex( int squareIndex )
        {
            Square square = new Square(squareIndex);

            if (square.File > 0 && square.Rank < 6)
            {
                KnightAttackTable[squareIndex].SetBitToOne( squareIndex + 15 );
            }
            if (square.File < 7 && square.Rank < 6)
            {
                KnightAttackTable[squareIndex].SetBitToOne( squareIndex + 17 );
            }
            if (square.File > 1 && square.Rank < 7)
            {
                KnightAttackTable[squareIndex].SetBitToOne( squareIndex + 6 );
            }
            if (square.File < 6 && square.Rank < 7)
            {
                KnightAttackTable[squareIndex].SetBitToOne( squareIndex + 10 );
            }
            if (square.File > 0 && square.Rank > 1)
            {
                KnightAttackTable[squareIndex].SetBitToOne( squareIndex - 17 );
            }
            if (square.File < 7 && square.Rank > 1)
            {
                KnightAttackTable[squareIndex].SetBitToOne( squareIndex - 15 );
            }
            if (square.File > 1 && square.Rank > 0)
            {
                KnightAttackTable[squareIndex].SetBitToOne( squareIndex - 10 );
            }
            if (square.File < 6 && square.Rank > 0)
            {
                KnightAttackTable[squareIndex].SetBitToOne( squareIndex - 6 );
            }
        }

        private static void InitializeKingAttacksForIndex( int squareIndex )
        {
            Square square = new Square(squareIndex);

            for (int rank = -1; rank < 2; rank++)
            {
                if (square.Rank + rank < 0 || square.Rank + rank > 7) continue;
                for (int file = -1; file < 2; file++)
                {
                    if ((rank | file) == 0) continue;
                    if (square.File + file < 0 || square.File + file > 7) continue;
                    KingAttacksTable[squareIndex].SetBitToOne( new Square( square.Rank + rank, square.File + file ) );
                }
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Bitboard GetBishopAttacks( int squareIndex, ulong blocker ) => _slidingPieceAttacks!.GetBishopAttacks( squareIndex, blocker );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Bitboard GetRookAttacks( int squareIndex, ulong blocker ) => _slidingPieceAttacks!.GetRookAttacks( squareIndex, blocker );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Bitboard GetQueenAttacks( int squareIndex, ulong blocker ) => 
            GetBishopAttacks( squareIndex, blocker ) | GetRookAttacks( squareIndex, blocker );

        public static bool IsSquareAttacked( int squareIndex, bool isSquareWhite, Board board )
        {
            Bitboard colorPawnAttack = isSquareWhite ? WhitePawnAttackTable[squareIndex] : BlackPawnAttackTable[squareIndex];
            Bitboard attackerPieces = isSquareWhite ? board.Data[6] : board.Data[0];
            if ((colorPawnAttack & attackerPieces) > 0) return true;
            attackerPieces = isSquareWhite ? board.Data[7] : board.Data[1];
            if ((KnightAttackTable[squareIndex] & attackerPieces) > 0) return true;
            attackerPieces = isSquareWhite ? board.Data[8] : board.Data[2];
            if ((GetBishopAttacks( squareIndex, board.Data[14] ) & attackerPieces) > 0) return true;
            attackerPieces = isSquareWhite ? board.Data[9] : board.Data[3];
            if ((GetRookAttacks( squareIndex, board.Data[14] ) & attackerPieces) > 0) return true;
            attackerPieces = isSquareWhite ? board.Data[10] : board.Data[4];
            if ((GetQueenAttacks( squareIndex, board.Data[14] ) & attackerPieces) > 0) return true;
            attackerPieces = isSquareWhite ? board.Data[11] : board.Data[5];
            if ((KingAttacksTable[squareIndex] & attackerPieces) > 0) return true;
            return false;
        }
    }
}

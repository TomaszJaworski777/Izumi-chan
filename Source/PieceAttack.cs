using System.Runtime.CompilerServices;

namespace Greg
{
    internal static class PieceAttack
    {
        public static Array64<Bitboard> WhitePawnAttackTable = default;
        public static Array64<Bitboard> BlackPawnAttackTable = default;
        public static Array64<Bitboard> KnightAttackTable = default;
        public static Array64<int> BishopRelevantBitCountForSquare = default;
        public static Array64<int> RookRelevantBitCountForSquare = default;
        public static Array64<Bitboard> BishopAttackMasks = default;
        public static Array64<Bitboard> RookAttackMasks = default;
        public static Array64<BishopAttacks> BishopAttacks = default;
        public static Array64<RookAttacks> RookAttacks = default;

        static PieceAttack()
        {
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                InitializePawnAttacksForIndex( squareIndex );
                InitializeKnightAttacksForIndex( squareIndex );
                BishopRelevantBitCountForSquare[squareIndex] = GetBishopRelevantBits( squareIndex ).BitCount;
                RookRelevantBitCountForSquare[squareIndex] = GetRookRelevantBits( squareIndex ).BitCount;
            }

            InitializeSlidingPieceAttacks();
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

        public static Bitboard GetBishopRelevantBits( int squareIndex )
        {
            Square square = new Square(squareIndex);
            Bitboard result = new();

            for (int file = square.File + 1, rank = square.Rank + 1; file < 7 && rank < 7; file++, rank++)
            {
                result.SetBitToOne( new Square( rank, file ).SquareIndex );
            }
            for (int file = square.File - 1, rank = square.Rank + 1; file > 0 && rank < 7; file--, rank++)
            {
                result.SetBitToOne( new Square( rank, file ).SquareIndex );
            }
            for (int file = square.File + 1, rank = square.Rank - 1; file < 7 && rank > 0; file++, rank--)
            {
                result.SetBitToOne( new Square( rank, file ).SquareIndex );
            }
            for (int file = square.File - 1, rank = square.Rank - 1; file > 0 && rank > 0; file--, rank--)
            {
                result.SetBitToOne( new Square( rank, file ).SquareIndex );
            }

            return result;
        }

        public static Bitboard GetRookRelevantBits( int squareIndex )
        {
            Square square = new Square(squareIndex);
            Bitboard result = new();

            for (int file = square.File + 1; file < 7; file++)
            {
                result.SetBitToOne( new Square( square.Rank, file ).SquareIndex );
            }
            for (int file = square.File - 1; file > 0; file--)
            {
                result.SetBitToOne( new Square( square.Rank, file ).SquareIndex );
            }
            for (int rank = square.Rank + 1; rank < 7; rank++)
            {
                result.SetBitToOne( new Square( rank, square.File ).SquareIndex );
            }
            for (int rank = square.Rank - 1; rank > 0; rank--)
            {
                result.SetBitToOne( new Square( rank, square.File ).SquareIndex );
            }

            return result;
        }

        public static Bitboard GetFullBishopAttackPattern( int squareIndex, ulong blocker )
        {
            Bitboard result = new();
            Square square = new(squareIndex);

            for (int file = square.File + 1, rank = square.Rank + 1; file <= 7 && rank <= 7; file++, rank++)
            {
                result.SetBitToOne( new Square( rank, file ).SquareIndex );
                if ((1UL << new Square( rank, file ).SquareIndex & blocker) > 0)
                    break;
            }
            for (int file = square.File - 1, rank = square.Rank + 1; file >= 0 && rank <= 7; file--, rank++)
            {
                result.SetBitToOne( new Square( rank, file ).SquareIndex );
                if ((1UL << new Square( rank, file ).SquareIndex & blocker) > 0)
                    break;
            }
            for (int file = square.File + 1, rank = square.Rank - 1; file <= 7 && rank >= 0; file++, rank--)
            {
                result.SetBitToOne( new Square( rank, file ).SquareIndex );
                if ((1UL << new Square( rank, file ).SquareIndex & blocker) > 0)
                    break;
            }
            for (int file = square.File - 1, rank = square.Rank - 1; file >= 0 && rank >= 0; file--, rank--)
            {
                result.SetBitToOne( new Square( rank, file ).SquareIndex );
                if ((1UL << new Square( rank, file ).SquareIndex & blocker) > 0)
                    break;
            }

            return result;
        }

        public static Bitboard GetFullRookAttackPattern( int squareIndex, ulong blocker )
        {
            Bitboard result = new();
            Square square = new(squareIndex);

            for (int file = square.File + 1; file <= 7; file++)
            {
                result.SetBitToOne( new Square( square.Rank, file ).SquareIndex );
                if ((1UL << new Square( square.Rank, file ).SquareIndex & blocker) > 0)
                    break;
            }
            for (int file = square.File - 1; file >= 0; file--)
            {
                result.SetBitToOne( new Square( square.Rank, file ).SquareIndex );
                if ((1UL << new Square( square.Rank, file ).SquareIndex & blocker) > 0)
                    break;
            }
            for (int rank = square.Rank + 1; rank <= 7; rank++)
            {
                result.SetBitToOne( new Square( rank, square.File ).SquareIndex );
                if ((1UL << new Square( rank, square.File ).SquareIndex & blocker) > 0)
                    break;
            }
            for (int rank = square.Rank - 1; rank >= 0; rank--)
            {
                result.SetBitToOne( new Square( rank, square.File ).SquareIndex );
                if ((1UL << new Square( rank, square.File ).SquareIndex & blocker) > 0)
                    break;
            }

            return result;
        }

        public static Bitboard SetOccupancy( int index, Bitboard attackMask )
        {
            Bitboard attackMaskCopy = attackMask;
            Bitboard result = new();

            for (int i = 0; i < attackMask.BitCount; i++)
            {
                int squareIndex = attackMaskCopy.LsbIndex;
                attackMaskCopy.SetBitToZero( squareIndex );

                if ((index & (1 << i)) > 0)
                    result.SetBitToOne( squareIndex );
            }

            return result;
        }

        public static Bitboard FindMagicNumber( int squareIndex, int relevantBitsCount, bool forBishop )
        {
            Span<Bitboard> occupancyPatterns = stackalloc Bitboard[4096];
            Span<Bitboard> attacks = stackalloc Bitboard[4096];
            Span<Bitboard> checkedAttacks = stackalloc Bitboard[4096];

            Bitboard attackMask = forBishop ? GetBishopRelevantBits(squareIndex) : GetRookRelevantBits(squareIndex);

            int occupancyIndexCount = 1 << relevantBitsCount;

            for (int index = 0; index < occupancyIndexCount; index++)
            {
                occupancyPatterns[index] = SetOccupancy( index, attackMask );
                attacks[index] = forBishop ? GetFullBishopAttackPattern( squareIndex, occupancyPatterns[index] ) : GetFullRookAttackPattern( squareIndex, occupancyPatterns[index] );
            }

            for (int iterationCount = 0; iterationCount < 1000000000; iterationCount++)
            {
                ulong magicNumber = Utils.Get64Random() & Utils.Get64Random() & Utils.Get64Random();

                if (((ulong)new Bitboard( (attackMask * magicNumber) & 0xFF00000000000000 ).BitCount) < 6)
                    continue;

                checkedAttacks.Clear();

                bool incorrectNumber = false;
                for (int index = 0; !incorrectNumber && index < occupancyIndexCount; index++)
                {
                    int magicIndex = (int)((occupancyPatterns[index] * magicNumber) >> (64 - relevantBitsCount));

                    if (checkedAttacks[magicIndex] == 0)
                        checkedAttacks[magicIndex] = attacks[index];
                    else if (checkedAttacks[magicIndex] != attacks[index])
                        incorrectNumber = true;
                }

                if (!incorrectNumber)
                    return new Bitboard( magicNumber );
            }

            return new();
        }

        public static void InitializeSlidingPieceAttacks()
        {
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                BishopAttackMasks[squareIndex] = GetBishopRelevantBits( squareIndex );
                RookAttackMasks[squareIndex] = GetRookRelevantBits( squareIndex );

                int occupancyIndexCount = 1 << BishopAttackMasks[squareIndex].BitCount;

                for (int index = 0; index < occupancyIndexCount; index++)
                {
                    Bitboard occupancy = SetOccupancy(index, BishopAttackMasks[squareIndex]);
                    int magicIndex = (int)((occupancy * MagicBitboards.BishopMagicNumbers[squareIndex]) >> (64 - BishopRelevantBitCountForSquare[squareIndex]));
                    BishopAttacks[squareIndex][magicIndex] = GetFullBishopAttackPattern( squareIndex, occupancy );
                }

                occupancyIndexCount = 1 << RookAttackMasks[squareIndex].BitCount;

                for (int index = 0; index < occupancyIndexCount; index++)
                {
                    Bitboard occupancy = SetOccupancy(index, RookAttackMasks[squareIndex]);
                    int magicIndex = (int)((occupancy * MagicBitboards.RookMagicNumbers[squareIndex]) >> (64 - RookRelevantBitCountForSquare[squareIndex]));
                    RookAttacks[squareIndex][magicIndex] = GetFullRookAttackPattern( squareIndex, occupancy );
                }
            }
        }

        public static Bitboard GetBishopAttacks(int squareIndex, ulong blocker )
        {
            blocker &= BishopAttackMasks[squareIndex];
            blocker *= MagicBitboards.BishopMagicNumbers[squareIndex];
            blocker >>= 64 - BishopRelevantBitCountForSquare[squareIndex];
            return BishopAttacks[squareIndex][(int)blocker];
        }

        public static Bitboard GetRookAttacks( int squareIndex, ulong blocker )
        {
            blocker &= RookAttackMasks[squareIndex];
            blocker *= MagicBitboards.RookMagicNumbers[squareIndex];
            blocker >>= 64 - RookRelevantBitCountForSquare[squareIndex];
            return RookAttacks[squareIndex][(int)blocker];
        }

        public static Bitboard GenerateAttackBitboard( bool forWhite )
        {
            Bitboard result = new();
            return result;
        }
    }

    [InlineArray( 512 )]
    internal struct BishopAttacks
    {
        private Bitboard _value;
    }

    [InlineArray( 4096 )]
    internal struct RookAttacks
    {
        private Bitboard _value;
    }
}

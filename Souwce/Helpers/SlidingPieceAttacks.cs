using System.Runtime.CompilerServices;
using Izumi.Structures;

namespace Izumi.Helpers
{
    internal class SlidingPieceAttacks
    {
        private Array64<int> _bishopRelevantBitCountForSquare = default;
        private Array64<int> _rookRelevantBitCountForSquare = default;
        private Array64<Bitboard> _bishopAttackMasks = default;
        private Array64<Bitboard> _rookAttackMasks = default;
        private Array64<BishopAttacks> _bishopAttacks = default;
        private Array64<RookAttacks> _rookAttacks = default;

        private readonly MagicBitboards _magicBitboards;

        public SlidingPieceAttacks()
        {
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                _bishopRelevantBitCountForSquare[squareIndex] = GetBishopRelevantBits(squareIndex).BitCount;
                _rookRelevantBitCountForSquare[squareIndex] = GetRookRelevantBits(squareIndex).BitCount;
            }

            _magicBitboards = new(_bishopRelevantBitCountForSquare, _rookRelevantBitCountForSquare, this);

            InitializeSlidingPieceAttacks();
        }

        public void InitializeSlidingPieceAttacks()
        {
            for (int squareIndex = 0; squareIndex < 64; squareIndex++)
            {
                _bishopAttackMasks[squareIndex] = GetBishopRelevantBits(squareIndex);
                _rookAttackMasks[squareIndex] = GetRookRelevantBits(squareIndex);

                int occupancyIndexCount = 1 << _bishopAttackMasks[squareIndex].BitCount;

                for (int index = 0; index < occupancyIndexCount; index++)
                {
                    Bitboard occupancy = SetOccupancy(index, _bishopAttackMasks[squareIndex]);
                    int magicIndex = (int)(occupancy * _magicBitboards.BishopMagicNumbers[squareIndex] >> 64 - _bishopRelevantBitCountForSquare[squareIndex]);
                    _bishopAttacks[squareIndex][magicIndex] = GetFullBishopAttackPattern(squareIndex, occupancy);
                }

                occupancyIndexCount = 1 << _rookAttackMasks[squareIndex].BitCount;

                for (int index = 0; index < occupancyIndexCount; index++)
                {
                    Bitboard occupancy = SetOccupancy(index, _rookAttackMasks[squareIndex]);
                    int magicIndex = (int)(occupancy * _magicBitboards.RookMagicNumbers[squareIndex] >> 64 - _rookRelevantBitCountForSquare[squareIndex]);
                    _rookAttacks[squareIndex][magicIndex] = GetFullRookAttackPattern(squareIndex, occupancy);
                }
            }
        }

        public Bitboard GetBishopRelevantBits(int squareIndex)
        {
            Square square = new Square(squareIndex);
            Bitboard result = new();

            for (int file = square.File + 1, rank = square.Rank + 1; file < 7 && rank < 7; file++, rank++)
            {
                result.SetBitToOne(new Square(rank, file).SquareIndex);
            }
            for (int file = square.File - 1, rank = square.Rank + 1; file > 0 && rank < 7; file--, rank++)
            {
                result.SetBitToOne(new Square(rank, file).SquareIndex);
            }
            for (int file = square.File + 1, rank = square.Rank - 1; file < 7 && rank > 0; file++, rank--)
            {
                result.SetBitToOne(new Square(rank, file).SquareIndex);
            }
            for (int file = square.File - 1, rank = square.Rank - 1; file > 0 && rank > 0; file--, rank--)
            {
                result.SetBitToOne(new Square(rank, file).SquareIndex);
            }

            return result;
        }

        public Bitboard GetRookRelevantBits(int squareIndex)
        {
            Square square = new Square(squareIndex);
            Bitboard result = new();

            for (int file = square.File + 1; file < 7; file++)
            {
                result.SetBitToOne(new Square(square.Rank, file).SquareIndex);
            }
            for (int file = square.File - 1; file > 0; file--)
            {
                result.SetBitToOne(new Square(square.Rank, file).SquareIndex);
            }
            for (int rank = square.Rank + 1; rank < 7; rank++)
            {
                result.SetBitToOne(new Square(rank, square.File).SquareIndex);
            }
            for (int rank = square.Rank - 1; rank > 0; rank--)
            {
                result.SetBitToOne(new Square(rank, square.File).SquareIndex);
            }

            return result;
        }

        public Bitboard GetFullBishopAttackPattern(int squareIndex, ulong blocker)
        {
            Bitboard result = new();
            Square square = new(squareIndex);

            for (int file = square.File + 1, rank = square.Rank + 1; file <= 7 && rank <= 7; file++, rank++)
            {
                result.SetBitToOne(new Square(rank, file).SquareIndex);
                if ((1UL << new Square(rank, file).SquareIndex & blocker) > 0)
                    break;
            }
            for (int file = square.File - 1, rank = square.Rank + 1; file >= 0 && rank <= 7; file--, rank++)
            {
                result.SetBitToOne(new Square(rank, file).SquareIndex);
                if ((1UL << new Square(rank, file).SquareIndex & blocker) > 0)
                    break;
            }
            for (int file = square.File + 1, rank = square.Rank - 1; file <= 7 && rank >= 0; file++, rank--)
            {
                result.SetBitToOne(new Square(rank, file).SquareIndex);
                if ((1UL << new Square(rank, file).SquareIndex & blocker) > 0)
                    break;
            }
            for (int file = square.File - 1, rank = square.Rank - 1; file >= 0 && rank >= 0; file--, rank--)
            {
                result.SetBitToOne(new Square(rank, file).SquareIndex);
                if ((1UL << new Square(rank, file).SquareIndex & blocker) > 0)
                    break;
            }

            return result;
        }

        public Bitboard GetFullRookAttackPattern(int squareIndex, ulong blocker)
        {
            Bitboard result = new();
            Square square = new(squareIndex);

            for (int file = square.File + 1; file <= 7; file++)
            {
                result.SetBitToOne(new Square(square.Rank, file).SquareIndex);
                if ((1UL << new Square(square.Rank, file).SquareIndex & blocker) > 0)
                    break;
            }
            for (int file = square.File - 1; file >= 0; file--)
            {
                result.SetBitToOne(new Square(square.Rank, file).SquareIndex);
                if ((1UL << new Square(square.Rank, file).SquareIndex & blocker) > 0)
                    break;
            }
            for (int rank = square.Rank + 1; rank <= 7; rank++)
            {
                result.SetBitToOne(new Square(rank, square.File).SquareIndex);
                if ((1UL << new Square(rank, square.File).SquareIndex & blocker) > 0)
                    break;
            }
            for (int rank = square.Rank - 1; rank >= 0; rank--)
            {
                result.SetBitToOne(new Square(rank, square.File).SquareIndex);
                if ((1UL << new Square(rank, square.File).SquareIndex & blocker) > 0)
                    break;
            }

            return result;
        }

        public Bitboard SetOccupancy(int index, Bitboard attackMask)
        {
            Bitboard attackMaskCopy = attackMask;
            Bitboard result = new();

            for (int i = 0; i < attackMask.BitCount; i++)
            {
                int squareIndex = attackMaskCopy.LsbIndex;
                attackMaskCopy.SetBitToZero(squareIndex);

                if ((index & 1 << i) > 0)
                    result.SetBitToOne(squareIndex);
            }

            return result;
        }

        public Bitboard GetBishopAttacks(int squareIndex, ulong blocker)
        {
            blocker &= _bishopAttackMasks[squareIndex];
            blocker *= _magicBitboards.BishopMagicNumbers[squareIndex];
            blocker >>= 64 - _bishopRelevantBitCountForSquare[squareIndex];
            return _bishopAttacks[squareIndex][(int)blocker];
        }

        public Bitboard GetRookAttacks(int squareIndex, ulong blocker)
        {
            blocker &= _rookAttackMasks[squareIndex];
            blocker *= _magicBitboards.RookMagicNumbers[squareIndex];
            blocker >>= 64 - _rookRelevantBitCountForSquare[squareIndex];
            return _rookAttacks[squareIndex][(int)blocker];
        }
    }


    [InlineArray(512)]
    internal struct BishopAttacks
    {
        private Bitboard _value;
    }

    [InlineArray(4096)]
    internal struct RookAttacks
    {
        private Bitboard _value;
    }
}

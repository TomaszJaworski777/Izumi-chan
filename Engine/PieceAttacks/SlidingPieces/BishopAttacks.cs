using System.Runtime.CompilerServices;
using Engine.Data.Bitboards;
using Engine.Utils;

namespace Engine.PieceAttacks.SlidingPieces;

public class BishopAttacks
{
    private Array64<int> _bishopRelevantBitCountForSquare;
    private Array64<Bitboard> _bishopAttackMasks;
    private Array64<BishopAttacksData> _bishopAttacks;

    public BishopAttacks()
    {
        for (int squareIndex = 0; squareIndex < 64; squareIndex++)
        {
            _bishopRelevantBitCountForSquare[squareIndex] = GetBishopRelevantBits( squareIndex ).BitCount;

            _bishopAttackMasks[squareIndex] = GetBishopRelevantBits( squareIndex );

            int occupancyIndexCount = 1 << _bishopAttackMasks[squareIndex].BitCount;

            for (int index = 0; index < occupancyIndexCount; index++)
            {
                Bitboard occupancy = SetOccupancy(index, _bishopAttackMasks[squareIndex]);
                int magicIndex = (int)(occupancy * MagicNumbers.BishopMagicNumbers[squareIndex] >> 64 - _bishopRelevantBitCountForSquare[squareIndex]);
                _bishopAttacks[squareIndex][magicIndex] = GetFullBishopAttackPattern( squareIndex, occupancy );
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    //creates attack mask that we will later use to create piece attack table
    public Bitboard GetBishopRelevantBits( int squareIndex )
    {
        int squareRank = SquareHelpers.SquareIndexToRank(squareIndex);
        int squareFile = SquareHelpers.SquareIndexToFile(squareIndex);
        Bitboard result = new();

        for (int file = squareFile + 1, rank = squareRank + 1; file < 7 && rank < 7; file++, rank++)
        {
            result.SetBitToOne( SquareHelpers.CoordsToSqaureIndex( rank, file ) );
        }
        for (int file = squareFile - 1, rank = squareRank + 1; file > 0 && rank < 7; file--, rank++)
        {
            result.SetBitToOne( SquareHelpers.CoordsToSqaureIndex( rank, file ) );
        }
        for (int file = squareFile + 1, rank = squareRank - 1; file < 7 && rank > 0; file++, rank--)
        {
            result.SetBitToOne( SquareHelpers.CoordsToSqaureIndex( rank, file ) );
        }
        for (int file = squareFile - 1, rank = squareRank - 1; file > 0 && rank > 0; file--, rank--)
        {
            result.SetBitToOne( SquareHelpers.CoordsToSqaureIndex( rank, file ) );
        }

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    //generates full attack pattern, we use that to store all possible attack patterns
    public Bitboard GetFullBishopAttackPattern( int squareIndex, ulong blocker )
    {
        Bitboard result = new();
        int squareRank = SquareHelpers.SquareIndexToRank(squareIndex);
        int squareFile = SquareHelpers.SquareIndexToFile(squareIndex);

        for (int file = squareFile + 1, rank = squareRank + 1; file <= 7 && rank <= 7; file++, rank++)
        {
            int index = SquareHelpers.CoordsToSqaureIndex(rank, file);
            result.SetBitToOne( index );
            if ((1UL << index & blocker) > 0)
                break;
        }
        for (int file = squareFile - 1, rank = squareRank + 1; file >= 0 && rank <= 7; file--, rank++)
        {
            int index = SquareHelpers.CoordsToSqaureIndex(rank, file);
            result.SetBitToOne( index );
            if ((1UL << index & blocker) > 0)
                break;
        }
        for (int file = squareFile + 1, rank = squareRank - 1; file <= 7 && rank >= 0; file++, rank--)
        {
            int index = SquareHelpers.CoordsToSqaureIndex(rank, file);
            result.SetBitToOne( index );
            if ((1UL << index & blocker) > 0)
                break;
        }
        for (int file = squareFile - 1, rank = squareRank - 1; file >= 0 && rank >= 0; file--, rank--)
        {
            int index = SquareHelpers.CoordsToSqaureIndex(rank, file);
            result.SetBitToOne( index );
            if ((1UL << index & blocker) > 0)
                break;
        }

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitboard SetOccupancy( int index, Bitboard attackMask )
    {
        Bitboard attackMaskCopy = attackMask;
        Bitboard result = new();

        for (int i = 0; i < attackMask.BitCount; i++)
        {
            int squareIndex = attackMaskCopy.LsbIndex;
            attackMaskCopy.SetBitToZero( squareIndex );

            if ((index & 1 << i) > 0)
                result.SetBitToOne( squareIndex );
        }

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bitboard GetAttacksForSquare( int squareIndex, ulong blocker )
    {
        blocker &= _bishopAttackMasks[squareIndex];
        blocker *= MagicNumbers.BishopMagicNumbers[squareIndex];
        blocker >>= 64 - _bishopRelevantBitCountForSquare[squareIndex];
        return _bishopAttacks[squareIndex][(int)blocker];
    }
}

[InlineArray( 512 )]
internal struct BishopAttacksData
{
    private Bitboard _value;
}

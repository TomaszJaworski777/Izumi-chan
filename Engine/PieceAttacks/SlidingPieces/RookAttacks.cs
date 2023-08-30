using System.Runtime.CompilerServices;
using Engine.Data.Bitboards;
using Engine.Utils;

namespace Engine.PieceAttacks.SlidingPieces;

public class RookAttacks
{
    private Array64<int> _rookRelevantBitCountForSquare;
    private Array64<Bitboard> _rookAttackMasks;
    private Array64<RookAttacksData> _rookAttacks;

    public RookAttacks()
    {
        for (int squareIndex = 0; squareIndex < 64; squareIndex++)
        {
            _rookRelevantBitCountForSquare[squareIndex] = GetRookRelevantBits( squareIndex ).BitCount;

            _rookAttackMasks[squareIndex] = GetRookRelevantBits( squareIndex );

            int occupancyIndexCount = 1 << _rookAttackMasks[squareIndex].BitCount;

            for (int index = 0; index < occupancyIndexCount; index++)
            {
                Bitboard occupancy = SetOccupancy(index, _rookAttackMasks[squareIndex]);
                int magicIndex = (int)(occupancy * MagicNumbers.RookMagicNumbers[squareIndex] >> 64 - _rookRelevantBitCountForSquare[squareIndex]);
                _rookAttacks[squareIndex][magicIndex] = GetFullRookAttackPattern( squareIndex, occupancy );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    //creates attack mask that we will later use to create piece attack table
    public Bitboard GetRookRelevantBits( int squareIndex )
    {
        int squareRank = SquareHelpers.SquareIndexToRank(squareIndex);
        int squareFile = SquareHelpers.SquareIndexToFile(squareIndex);
        Bitboard result = new();

        for (int file = squareFile + 1; file < 7; file++)
        {
            result.SetBitToOne( SquareHelpers.CoordsToSqaureIndex( squareRank, file ) );
        }
        for (int file = squareFile - 1; file > 0; file--)
        {
            result.SetBitToOne( SquareHelpers.CoordsToSqaureIndex( squareRank, file ) );
        }
        for (int rank = squareRank + 1; rank < 7; rank++)
        {
            result.SetBitToOne( SquareHelpers.CoordsToSqaureIndex( rank, squareFile ) );
        }
        for (int rank = squareRank - 1; rank > 0; rank--)
        {
            result.SetBitToOne( SquareHelpers.CoordsToSqaureIndex( rank, squareFile ) );
        }

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    //generates full attack pattern, we use that to store all possible attack patterns
    public Bitboard GetFullRookAttackPattern( int squareIndex, ulong blocker )
    {
        Bitboard result = new();
        int squareRank = SquareHelpers.SquareIndexToRank(squareIndex);
        int squareFile = SquareHelpers.SquareIndexToFile(squareIndex);

        for (int file = squareFile + 1; file <= 7; file++)
        {
            int index = SquareHelpers.CoordsToSqaureIndex(squareRank, file );
            result.SetBitToOne( index );
            if ((1UL << index & blocker) > 0)
                break;
        }
        for (int file = squareFile - 1; file >= 0; file--)
        {
            int index = SquareHelpers.CoordsToSqaureIndex(squareRank, file );
            result.SetBitToOne( index );
            if ((1UL << index & blocker) > 0)
                break;
        }
        for (int rank = squareRank + 1; rank <= 7; rank++)
        {
            int index = SquareHelpers.CoordsToSqaureIndex(rank, squareFile );
            result.SetBitToOne( index );
            if ((1UL << index & blocker) > 0)
                break;
        }
        for (int rank = squareRank - 1; rank >= 0; rank--)
        {
            int index = SquareHelpers.CoordsToSqaureIndex(rank, squareFile );
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
        blocker &= _rookAttackMasks[squareIndex];
        blocker *= MagicNumbers.RookMagicNumbers[squareIndex];
        blocker >>= 64 - _rookRelevantBitCountForSquare[squareIndex];
        return _rookAttacks[squareIndex][(int)blocker];
    }
}

[InlineArray( 4096 )]
internal struct RookAttacksData
{
    private Bitboard _value;
}
namespace Greg
{
    internal class MagicBitboards
    {
        public Array64<ulong> BishopMagicNumbers = default; 
        public Array64<ulong> RookMagicNumbers = default;

        private readonly SlidingPieceAttacks _slidingPieceAttacks;

        public MagicBitboards(Array64<int> bishopRelevantBitCount, Array64<int> rookRelevantBitCount, SlidingPieceAttacks slidingPieceAttacks)
        {
            _slidingPieceAttacks = slidingPieceAttacks;

            for (int i = 0; i < 64; i++)
            {
                BishopMagicNumbers[i] = FindMagicNumber( i, bishopRelevantBitCount[i], true ).Value;
                RookMagicNumbers[i] = FindMagicNumber( i, rookRelevantBitCount[i], false ).Value;
            }
        }

        private Bitboard FindMagicNumber( int squareIndex, int relevantBitsCount, bool forBishop )
        {
            Span<Bitboard> occupancyPatterns = stackalloc Bitboard[4096];
            Span<Bitboard> attacks = stackalloc Bitboard[4096];
            Span<Bitboard> checkedAttacks = stackalloc Bitboard[4096];

            Bitboard attackMask = forBishop ? _slidingPieceAttacks.GetBishopRelevantBits(squareIndex) : _slidingPieceAttacks.GetRookRelevantBits(squareIndex);

            int occupancyIndexCount = 1 << relevantBitsCount;

            for (int index = 0; index < occupancyIndexCount; index++)
            {
                occupancyPatterns[index] = _slidingPieceAttacks.SetOccupancy( index, attackMask );
                attacks[index] = forBishop ? _slidingPieceAttacks.GetFullBishopAttackPattern( squareIndex, occupancyPatterns[index] ) : _slidingPieceAttacks.GetFullRookAttackPattern( squareIndex, occupancyPatterns[index] );
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
    }
}

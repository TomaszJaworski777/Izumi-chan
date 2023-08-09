using System.Runtime.CompilerServices;

namespace Greg
{
    internal struct Bitboard
    {
        public ulong Mask;

        public Bitboard( ulong mask = 0 )
        {
            Mask = mask;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ulong GetBitValue( int index ) => Mask & (1UL << index);

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetBitToOne( int index ) => Mask |= 1UL << index;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetBitToZero( int index ) => Mask &= ~(1UL << index);

        public Stack64<int> GetSetBitsIndexStack() //test if its actually working
        {
            Stack64<int> result = new();
            Bitboard bitboard = this;
            while (bitboard.Mask != 0)
            {
                int lsbIndex = bitboard.GetIndexOfLSB();
                result.Push( lsbIndex );
                bitboard.SetBitToZero( lsbIndex );
            }
            return result;
        }

        public int GetIndexOfLSB()
        {
            for (int i = 0; i < 64; i++)
                if (GetBitValue( i ) > 0)
                    return i;
            return -1;
        }

        public void Draw()
        {
            for (int i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                    Console.WriteLine();

                Console.Write( string.Format("{0, 2}", (GetBitValue( i^56 ) > 0) ? 'X' : '*' ) );
            }
            Console.WriteLine();
        }
    }
}

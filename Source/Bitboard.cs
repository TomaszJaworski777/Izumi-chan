using System.Runtime.CompilerServices;

namespace Greg
{
    internal struct Bitboard
    {
        public ulong Mask;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Bitboard( ulong mask = 0 ) => Mask = mask;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ulong GetBitValue( int index ) => Mask & (1UL << index);

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetBitToOne( int index ) => Mask |= 1UL << index;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetBitToZero( int index ) => Mask &= ~(1UL << index);

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ulong GetValueChunk( int index, ulong mask ) => (Mask & (mask << index)) >> index;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetValueChunk( int index, ulong mask, ulong newValue ) => Mask = (Mask & ~(mask << index)) | newValue << index;

        public void Draw()
        {
            for (int i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                    Console.WriteLine();

                Console.Write( string.Format( "{0, 2}", (GetBitValue( i ^ 56 ) > 0) ? 'X' : '*' ) );
            }
            Console.WriteLine();
        }
    }
}

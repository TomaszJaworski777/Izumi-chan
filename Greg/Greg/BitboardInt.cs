using System.Runtime.CompilerServices;

namespace Greg
{
    internal struct BitboardInt
    {
        public int Mask;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public BitboardInt( int mask = 0 ) => Mask = mask;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int GetBitValue( int index ) => Mask & (1 << index);

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetBitToOne( int index ) => Mask |= 1 << index;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetBitToZero( int index ) => Mask &= ~(1 << index);

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int GetValueChunk( int index, int mask ) => (Mask & (mask << index)) >> index;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetValueChunk( int index, int mask, int newValue ) => Mask = (Mask & ~(mask << index)) | newValue << index;

        public void Draw()
        {
            for (int i = 0; i < 32; i++)
            {
                if (i % 8 == 0)
                    Console.WriteLine();

                Console.Write( string.Format( "{0, 2}", (GetBitValue( i ^ 56 ) > 0) ? 'X' : '*' ) );
            }
            Console.WriteLine();
        }
    }
}

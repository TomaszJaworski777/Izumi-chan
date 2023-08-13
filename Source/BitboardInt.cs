using System.Runtime.CompilerServices;

namespace Greg
{
    internal struct BitboardInt
    {
        public int Value;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public BitboardInt( int mask = 0 ) => Value = mask;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int GetBitValue( int index ) => Value & (1 << index);

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetBitToOne( int index ) => Value |= 1 << index;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetBitToZero( int index ) => Value &= ~(1 << index);

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int GetValueChunk( int index, int mask ) => (Value & (mask << index)) >> index;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void SetValueChunk( int index, int mask, int newValue ) => Value = (Value & ~(mask << index)) | newValue << index;
        public int BitCount()
        {
            int maskCopy = Value;
            int result = 0;
            while (maskCopy > 0)
            {
                maskCopy &= maskCopy - 1;
                result++;
            }

            return result;
        }

        public void Draw()
        {
            for (int i = 0; i < 32; i++)
            {
                if (i % 8 == 0)
                    Console.WriteLine();

                Console.ForegroundColor = GetBitValue( i ^ 56 ) > 0 ? ConsoleColor.Green : ConsoleColor.Gray;
                Console.Write( string.Format( "{0, 2}", (GetBitValue( i ^ 56 ) > 0) ? 'X' : '*' ) );
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine( $"\nValue: {Value}" );
        }

        public static implicit operator int( BitboardInt bitboard ) => bitboard.Value;
        public static implicit operator BitboardInt( int mask ) => new BitboardInt( mask );
    }
}

using System.Numerics;
using System.Runtime.CompilerServices;

namespace Izumi.Structures.Data
{
    internal struct Bitboard
    {
        public ulong Value;

        public int BitCount
        {
            get
            {
                ulong maskCopy = Value;
                int result = 0;
                while (maskCopy > 0)
                {
                    maskCopy &= maskCopy - 1;
                    result++;
                }

                return result;
            }
        }

        public int LsbIndex => BitOperations.TrailingZeroCount(Value);
        public int MsbIndex => BitOperations.LeadingZeroCount(Value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitboard(ulong mask = 0) => Value = mask;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetBitValue(int index) => Value & 1UL << index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBitToOne(int index) => Value |= 1UL << index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetBitToZero(int index) => Value &= ~(1UL << index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetValueChunk(int index, ulong mask) => (Value & mask) >> index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValueChunk(int index, ulong mask, ulong newValue) => Value = (Value & ~mask) | newValue << index;

        public void Draw()
        {
            for (int i = 0; i < 64; i++)
            {
                if (i % 8 == 0)
                    Console.WriteLine();

                Console.ForegroundColor = GetBitValue(i ^ 56) > 0 ? ConsoleColor.Green : ConsoleColor.Gray;
                Console.Write(string.Format("{0, 2}", GetBitValue(i ^ 56) > 0 ? 'X' : '*'));
            }
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"\nValue: {Value}");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ulong(Bitboard bitboard) => bitboard.Value;
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static implicit operator Bitboard(ulong mask) => new Bitboard(mask);
    }
}

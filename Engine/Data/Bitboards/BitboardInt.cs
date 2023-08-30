using System.Numerics;
using System.Runtime.CompilerServices;

namespace Engine.Data.Bitboards;

public struct BitboardInt
{
    private int _value;

    public int BitCount => BitOperations.PopCount((uint)_value);
    public int LsbIndex => BitOperations.TrailingZeroCount( _value );
    public int MsbIndex => BitOperations.LeadingZeroCount( (uint)_value );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private BitboardInt( int value )
    {
        _value = value;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int GetBitValue( int index ) => _value & 1 << index;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetBitToOne( int index ) => _value |= 1 << index;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetBitToZero( int index ) => _value &= ~(1 << index);

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int GetValueChunk( int index, int mask ) => (_value & (mask << index)) >> index;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetValueChunk( int index, int mask, int newValue ) => _value = (_value & ~(mask << index)) | newValue << index;

    public void Draw()
    {
        for (int i = 0; i < 64; i++)
        {
            if (i % 8 == 0)
                Console.WriteLine();

            Console.ForegroundColor = GetBitValue( i ^ 56 ) > 0 ? ConsoleColor.Green : ConsoleColor.Gray;
            Console.Write("{0, 2}", GetBitValue( i ^ 56 ) > 0 ? 'X' : '*');
        }
        Console.ForegroundColor = ConsoleColor.Gray;
        Console.WriteLine( $"\nValue: {_value}" );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator BitboardInt( int value ) => new( value );
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator int( BitboardInt bitboard ) => bitboard._value;
}
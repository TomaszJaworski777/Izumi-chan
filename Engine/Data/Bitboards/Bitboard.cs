using System.Numerics;
using System.Runtime.CompilerServices;

namespace Engine.Data.Bitboards;

public struct Bitboard
{
    private ulong _value;

    public int BitCount => BitOperations.PopCount( _value );
    public int LsbIndex => BitOperations.TrailingZeroCount( _value );
    public int MsbIndex => BitOperations.LeadingZeroCount( _value );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Bitboard( ulong value )
    {
        _value = value;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ulong GetBitValue( int index ) => _value & (1UL << index);

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetBitToOne( int index ) => _value |= 1UL << index;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetBitToZero( int index ) => _value &= ~(1UL << index);

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ulong GetValueChunk( int index, ulong mask ) => (_value & (mask << index)) >> index;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetValueChunk( int index, ulong mask, ulong newValue ) => _value = (_value & ~(mask << index)) | newValue << index;

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
    public static implicit operator Bitboard( ulong value ) => new( value );
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator ulong( Bitboard bitboard ) => bitboard._value;
}
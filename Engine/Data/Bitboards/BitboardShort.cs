using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Engine.Data.Bitboards;

public struct BitboardShort
{
    private ushort _value;

    public int BitCount => BitOperations.PopCount( _value );
    public int LsbIndex => BitOperations.TrailingZeroCount( _value );
    public int MsbIndex => BitOperations.LeadingZeroCount( _value );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private BitboardShort( ushort value )
    {
        _value = value;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ushort GetBitValue( int index ) => (ushort)(_value & (1 << index));

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetBitToOne( int index ) => _value |= (ushort)(1 << index);

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetBitToZero( int index ) => _value &= (ushort)~(1 << index);

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ushort GetValueChunk( int index, ushort mask ) => (ushort)((_value & (mask << index)) >> index);

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetValueChunk( int index, ushort mask, ushort newValue ) => _value = (ushort)((_value & ~(mask << index)) | newValue << index);

    public void Draw()
    {
        for (int i = 0; i < 16; i++)
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
    public static implicit operator BitboardShort( ushort value ) => new( value );
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator ushort( BitboardShort bitboard ) => bitboard._value;
}
using System;
using System.Runtime.CompilerServices;

namespace Engine.Utils;

public static class SquareHelpers
{
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static int StringToSquareIndex( ReadOnlySpan<char> square ) => CoordsToSqaureIndex( int.Parse( new ReadOnlySpan<char>(in square[1]) ) - 1, square[0] - 'a' );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static int SquareIndexToRank( int index ) => index / 8;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static int SquareIndexToFile( int index ) => index % 8;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static int CoordsToSqaureIndex( int rank, int file ) => rank * 8 + file;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static string SquareIndexToString( int index ) => $"{(char)('a' + SquareIndexToFile( index ))}{SquareIndexToRank( index ) + 1}";
}

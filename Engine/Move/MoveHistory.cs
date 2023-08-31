using System;
using System.Runtime.CompilerServices;

namespace Engine.Move;

public static class MoveHistory
{
    private static HistoryList _list;
    private static int _count;

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void Reset()
    {
        _count = 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void Add( ulong key )
    {
        _list[_count] = key;
        _count++;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void RemoveLast()
    {
        _count--;
        if (_count == -1)
            Console.WriteLine( "?" );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsRepetition( ulong key )
    {
        int count = 0;

        for (int i = 0; i < _count; i++)
        {
            if (_list[i] == key)
                count++;
        }

        return count > 1;
    }
}

[InlineArray( 1024 )]
internal struct HistoryList
{
    private ulong _value;
}

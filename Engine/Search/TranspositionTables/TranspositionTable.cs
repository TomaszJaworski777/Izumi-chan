using Engine.Options;

namespace Engine.Search.TranspositionTables;

public static unsafe class TranspositionTable
{
    private const int BytesInMegabyte = 1048576;

    private static TranspositionTableEntry[] _entries;

    static TranspositionTable() => _entries = new TranspositionTableEntry[(int)EngineOptions.GetOption(EngineOptions.HashKey).Value * BytesInMegabyte / sizeof( TranspositionTableEntry )];

    public static void Reset()
    {
        _entries = new TranspositionTableEntry[(int)EngineOptions.GetOption( EngineOptions.HashKey ).Value * BytesInMegabyte / sizeof( TranspositionTableEntry )];
    }

    public static ref TranspositionTableEntry Probe( ulong key ) => ref _entries[GetIndex( ref key )];
    public static void Push( TranspositionTableEntry newEntry )
    {
        ref TranspositionTableEntry oldEntry = ref newEntry;
        if (oldEntry.PositionKey != newEntry.PositionKey || oldEntry.Depth - 3 < newEntry.Depth || newEntry.Flag == TTFlag.AlphaChanged)
            _entries[GetIndex( ref newEntry.PositionKey )] = newEntry;
    }

    private static int GetIndex( ref ulong key ) => (int)(key % (ulong)_entries.Length);
}
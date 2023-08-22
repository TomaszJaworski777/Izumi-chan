using Izumi.Structures.Data;

namespace Izumi.Structures
{
    internal static class TranspositionTable
    {
        private static TranspositionTableEntry[] _table = new TranspositionTableEntry[0];
        private static int _hashfullCounter = 0;

        public static int HashFull => (int)(_hashfullCounter / (float)_table.Length * 1000);

        static TranspositionTable()
        {
            ResizeTable(16);
        }

        public static void ResizeTable(int megabytes)
        {
            int bitAmount = megabytes * 1024 * 1024;
            _table = new TranspositionTableEntry[bitAmount / 120];
            _hashfullCounter = 0;
        }

        public static void Clear()
        {
            _hashfullCounter = 0;
        }

        public static TranspositionTableEntry? Probe(ulong key)
        {
            int index = GetIndexFromKey(key);
            TranspositionTableEntry entry = _table[index];
            if (entry.PositionKey == key)
                return entry;
            return null;
        }

        public static void Add(TranspositionTableEntry entry)
        {
            int index = GetIndexFromKey(entry.PositionKey);
            if (_table[index].PositionKey == 0)
                _hashfullCounter++;
            _table[index] = entry;
        }

        private static int GetIndexFromKey(ulong key) => (int)(key % (ulong)_table.Length);
    }

    internal struct TranspositionTableEntry //120 bits = 15 bytes
    {
        public short Score; //16-bit
        public byte Depth; //4-bit
        public ulong PositionKey; //64-bit
        public TTeEntryFlag Flag; //4-bit
        public Move bestMove; //32-bit
    }

    internal enum TTeEntryFlag : byte
    {
        Exact,
        Lowerbound,
        Upperbound
    }
}

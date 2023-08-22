using System.Runtime.CompilerServices;

namespace Izumi.Structures
{
    internal struct MoveHistory
    {
        private HistoryList _list;
        private int _count;

        public MoveHistory()
        {
            _list = default;
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Reset()
        {
            _list = default;
            _count = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Add(ulong key)
        {
            _list[_count] = key;
            _count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsRepetition(ulong key)
        {
            int count = 0;

            for (int i = 0; i < _count; i++)
            {
                if (_list[i] == key)
                    count++;
            }

            return count > 2;
        }
    }

    [InlineArray(128)]
    internal struct HistoryList
    {
        private ulong _value;
    }
}

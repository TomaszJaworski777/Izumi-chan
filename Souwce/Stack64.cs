using System.Runtime.CompilerServices;

namespace Izumi
{
    internal struct Stack64<T>
    {
        private Array64<T> _data;

        public int Length { get; private set; }

        public Stack64() => _data = default;

        public void Push(T value)
        {
            if (Length == 64)
                return;

            _data[Length] = value;
            Length++;
        }

        public T? Pop()
        {
            if (Length <= 0) 
                return default;
            Length--;
            return _data[Length];
        }

        public T this[int index]
        {
            readonly get => _data[index];
            set
            {
                if (index < Length)
                    _data[index] = value;
            }
        }
    }

    [InlineArray( 64 )]
    internal struct Array64<T>
    {
        private T _value;
    }
}

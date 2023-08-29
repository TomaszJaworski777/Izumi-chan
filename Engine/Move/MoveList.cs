using System.Runtime.CompilerServices;

namespace Engine.Move
{
    public unsafe ref struct MoveList
    {
        private Span<MoveData> _moves;
        private int _moveCount;

        public int Length => _moveCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        //moves will be declared in main scope an passed here as stackalloced empty span
        public MoveList( Span<MoveData> moves )
        {
            _moves = moves;
            _moveCount = 0;
        }
        
        public void Add(MoveData move )
        {
            _moves[_moveCount] = move;
            _moveCount++;
        }

        public readonly MoveData this[int index] => _moves[index];

        public static implicit operator Span<MoveData>( MoveList list ) => list._moves[..list._moveCount];
    }
}

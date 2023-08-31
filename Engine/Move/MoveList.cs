using System;
using System.Runtime.CompilerServices;

namespace Engine.Move
{
    [method: MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ref struct MoveList(Span<MoveData> moves)
    {
        private Span<MoveData> _moves = moves;
        public int Length { get; private set; } = 0;

        //moves will be declared in main scope an passed here as stackalloced empty span

        public void Add(MoveData move )
        {
            _moves[Length] = move;
            Length++;
        }

        public void ForceSetLength( int length ) => Length = length;

        public static implicit operator Span<MoveData>( MoveList list ) => list._moves[..list.Length];
    }
}

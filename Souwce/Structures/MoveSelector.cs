using Izumi.Structures.Data;
using System.Runtime.CompilerServices;

namespace Izumi.Structures
{
    internal unsafe ref struct MoveSelector
    {
        private Span<ScoredMove> _moves;
        private TranspositionTableEntry? _entry;

        public int Length { get; private set; }

        private struct ScoredMove
        {
            public Move Move;
            public int Score;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public MoveSelector( Span<Move> moves, TranspositionTableEntry? entry )
        {
            Length = moves.Length;
            _entry = entry;
            _moves = new ScoredMove[Length];
            fixed (ScoredMove* move = _moves)
            {
                Move bufferMove;
                for (int i = 0; i < _moves.Length; i++)
                {
                    ScoredMove* currentMove = move + i;
                    bufferMove = moves[i];
                    currentMove->Move = bufferMove;
                    currentMove->Score = GetMoveValue( bufferMove );
                }
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Move GetMoveForIndex( int index )
        {
            Move bestMove = _moves[index].Move;
            int currentScore = _moves[index].Score;
            int bestIndex = index;

            fixed (ScoredMove* move = _moves)
            {
                for (int i = index + 1; i < _moves.Length; i++)
                {
                    ScoredMove* currentMove = move + i;
                    int score = currentMove->Score;
                    if(score > currentScore)
                    {
                        currentScore = score;
                        bestIndex = i;
                        bestMove = currentMove->Move;
                    }
                }

                if(bestIndex != index)
                {
                    ScoredMove* buffer = move + index;
                    _moves[index] = _moves[bestIndex];
                    _moves[bestIndex] = *buffer;
                }
            }

            return bestMove;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private int GetMoveValue( Move move )
        {
            if (_entry != null && move.Equals( _entry.Value.bestMove ))
                return int.MaxValue;

            int result = 0;

            if (move.IsCapture)
                result += ((int)move.TargetPiece + 1) * 100 - (int)move.MovingPiece;
            if (move.IsPromotion)
                result += ((int)move.PromotionPiece + 1) * 100;
            if (move.IsCastle)
                result += 1;

            return result;
        }
    }
}

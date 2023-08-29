using System.Runtime.CompilerServices;

namespace Engine.Move;

//move ordering, in order to increase beta cutoffs in Negamax. (https://www.chessprogramming.org/Move_Ordering) Here i'm using iterative sorting method, to save performance
internal unsafe ref struct MoveSelector
{
    private Span<ScoredMove> _moves;

    public int Length { get; private set; }

    public struct ScoredMove
    {
        public MoveData Move;
        public int Score;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    //score the list of moves for future sort
    public MoveSelector( MoveList moves, Span<ScoredMove> alloc/*, TranspositionTableEntry? entry*/ )
    {
        Length = moves.Length;
        _moves = alloc;

        fixed (ScoredMove* scoredMovesPtr = _moves)
        {
            ScoredMove* currentMove = scoredMovesPtr;
            MoveData bufferMove;
            for (int i = 0; i < Length; i++)
            {
                bufferMove = moves[i];
                currentMove->Move = bufferMove;
                currentMove->Score = GetMoveValue( bufferMove/*, entry*/ );
                currentMove++;
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    //sort one move of the move list and returns it
    public MoveData GetMoveForIndex( int index )
    {
        MoveData bestMove = _moves[index].Move;
        int bestScore = _moves[index].Score;
        int bestIndex = index;

        fixed (ScoredMove* move = &_moves[0])
        {
            ScoredMove* currentMove = move + index + 1;
            ScoredMove* end = move + _moves.Length;

            while (currentMove < end)
            {
                int score = currentMove->Score;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestIndex = (int)(currentMove - move);
                    bestMove = currentMove->Move;
                }
                currentMove++;
            }

            if (bestIndex != index)
            {
                ScoredMove buffer = *(move + index);
                _moves[index] = _moves[bestIndex];
                _moves[bestIndex] = buffer;
            }
        }

        return bestMove;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    //scores move
    private int GetMoveValue( MoveData move/*, TranspositionTableEntry? entry*/ )
    {
/*        if (entry != null && move.Equals( entry.Value.bestMove ))
            return int.MaxValue;*/

        int result = 0;

        if (move.IsCapture)
            result += ((int)move.TargetPieceType + 1) * 100 - (int)move.MovingPieceType; //MVV-LVA (https://www.chessprogramming.org/MVV-LVA)
        if (move.IsPromotion)
            result += ((int)move.PromotionPieceType + 1) * 100;
        if (move.IsCastle)
            result += 1;

        return result;
    }
}
﻿using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Engine.Move;

//move ordering, in order to increase beta cutoffs in Negamax. (https://www.chessprogramming.org/Move_Ordering) Here i'm using iterative sorting method, to save performance
public readonly ref struct MoveSelector
{
    private readonly Span<ScoredMove> _moves;

    public int Length => _moves.Length;

    public struct ScoredMove
    {
        public MoveData Move;
        public int Score;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    //score the list of moves for future sort
    public MoveSelector( MoveList moves, Span<ScoredMove> alloc/*, TranspositionTableEntry? entry*/ )
    {
        _moves = alloc[..moves.Length];
        ref ScoredMove currentMove = ref MemoryMarshal.GetReference(_moves);

        foreach (ref MoveData bufferMove in (Span<MoveData>)moves)
        {
            currentMove.Move = bufferMove;
            currentMove.Score = GetMoveValue( bufferMove/*, entry*/ );
            currentMove = ref Unsafe.Add(ref currentMove, 1);
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    //sort one move of the move list and returns it
    public MoveData GetMoveForIndex( int index )
    {
        Span<ScoredMove> moves = _moves[index..];
        MoveData bestMove = moves[0].Move;
        int bestScore = moves[0].Score;
        int bestIndex = 0;

        for (int i = 0; i < moves.Length; i++)
        {
            ref ScoredMove move = ref moves[i];
            int score = move.Score;

            if (score > bestScore)
            {
                bestScore = score;
                bestIndex = i;
                bestMove = move.Move;
            }
        }

        if (bestIndex != 0)
        {
            (moves[0], moves[bestIndex]) = (moves[bestIndex], moves[0]);
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

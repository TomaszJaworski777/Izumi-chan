using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Engine.Board;
using Engine.Evaluation;
using Engine.Move;
using Engine.Perft;
using Engine.Search;

namespace Engine;

public class ChessEngine
{
    private string _currentPositionRootFEN = BoardProvider.StartPosition;
    private List<MoveData> _movesFromRoot = new();

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ChessEngine()
    {
        Console.WriteLine( EngineCredentials.Header );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void ChangePosition( string fen, List<MoveData> moves )
    {
        _currentPositionRootFEN = fen;
        _movesFromRoot = moves;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ulong Perft( string fen, int depth, bool divide )
    {
        BoardData board = BoardProvider.Create(fen);
        PerftTest test = new(ref board);
        return test.TestPosition( depth, divide );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ulong Perft( int depth, bool divide )
    {
        BoardData board = CreateCurrentBoard();
        PerftTest test = new(ref board);
        return test.TestPosition( depth, divide );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MoveData FindBestMove( int depth, int whiteTime, int blackTime, int whiteIncrement, int blackIncrement, int movesToGo )
    {
        BoardData board = CreateCurrentBoard();

        SearchSystem search = new();
        return search.FindBestMove( new SearchParameters( ref board, depth, whiteTime, blackTime, whiteIncrement, blackIncrement, movesToGo ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void InterruptLoops()
    {
        SearchSystem.CancellationToken = true;
        PerftTest.CancellationToken = true;
    }

    public void DebugEval()
    {
        BoardData board = CreateCurrentBoard();
        Console.WriteLine( $"Total evaluation: {EvaluationSystem.EvaluatePosition(ref board, true )}" );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void DrawBoard() => CreateCurrentBoard().Draw();

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private BoardData CreateCurrentBoard()
    {
        BoardData board = BoardProvider.Create(_currentPositionRootFEN);
        foreach (ref MoveData move in CollectionsMarshal.AsSpan(_movesFromRoot))
            board.MakeMove( move );
        return board;
    }
}

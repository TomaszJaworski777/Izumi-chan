using Engine.Board;
using Engine.Evaluation;
using Engine.Move;
using Engine.Perft;
using Engine.Search;
using System.Runtime.CompilerServices;

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
        PerftTest test = new PerftTest(board);
        return test.TestPosition( depth, divide );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ulong Perft( int depth, bool divide )
    {
        BoardData board = CreateCurrentBoard();
        PerftTest test = new PerftTest(board);
        return test.TestPosition( depth, divide );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MoveData FindBestMove( int depth, int whiteTime, int blackTime, int whiteIncrement, int blackIncrement, int movesToGo )
    {
        BoardData board = CreateCurrentBoard();

        SearchSystem search = new();
        return search.FindBestMove( new SearchParameters( board, depth, whiteTime, blackTime, whiteIncrement, blackIncrement, movesToGo ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void InterruptLoops()
    {
        SearchSystem.CancellationToken = true;
        PerftTest.CancellationToken = true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void DrawBoard() => CreateCurrentBoard().Draw();

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private BoardData CreateCurrentBoard()
    {
        BoardData board = BoardProvider.Create(_currentPositionRootFEN);
        foreach (MoveData move in _movesFromRoot)
            board.MakeMove( move );
        return board;
    }

    private static void Main( string[] args ) {}
}

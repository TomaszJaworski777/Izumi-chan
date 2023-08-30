using Engine;
using Engine.Board;
using Engine.Move;
using Engine.Options;
using Engine.Search;

namespace Interface;

internal class UciCommandProcessor : CommandProcessor
{
    public UciCommandProcessor( ChessEngine chessEngine ) : base( chessEngine ) 
    {
        Console.WriteLine( $"id name {EngineCredentials.FullName}" );
        Console.WriteLine( $"id author {EngineCredentials.Author}" );

        foreach (var option in EngineOptions.AllValues)
        {
            Console.Write( $"option name {option.Key} type {option.Value.Type.ToString().ToLowerInvariant()} default {option.Value.Value}" );

            if (option.Value.Type is OptionValueType.Spin)
                Console.Write( $" min {option.Value.MinValue} max {option.Value.MaxValue}" );

            Console.WriteLine();
        }

        Console.WriteLine( "uciok" );
    }

    public override void ProcessCommand( string[] commandSplit )
    {
        switch (commandSplit[0])
        {
            case "setoption":
                HandleSetOptionCommand( commandSplit[1..] );
                break;
            case "ucinewgame":
                HandleNewGameCommand();
                break;
            case "isready":
                HandleIsReadyCommand();
                break;
            case "position":
                HandlePositionCommand( commandSplit[1..] );
                break;
            case "go":
                HandleGoCommand( commandSplit[1..] );
                break;
        }
    }

    public void HandleSetOptionCommand( string[] args )
    {
        string name = "";
        string value = "";

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "name")
                name = args[i + 1];
            else if (args[i] == "value")
                value = args[i + 1];
        }

        EngineOptions.ChangeOption( name, value );
    }

    private void HandleNewGameCommand()
    {
        MoveHistory.Reset();
        //TT reset
    }

    private void HandleIsReadyCommand()
    {
        Console.WriteLine( "readyok" );
    }

    private void HandlePositionCommand( string[] args )
    {
        string fen = BoardProvider.StartPosition;
        List<MoveData>? moves = null;

        if (args[0] != "startpos")
            fen = args[1] + ' ' + args[2] + ' ' + args[3] + ' ' + args[4] + ' ' + args[5] + ' ' + args[6];

        BoardData tempBoard = BoardProvider.Create(fen);

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "moves")
            {
                moves = new();
                continue;
            }

            if (moves != null)
            {
                MoveData newMove = new ( args[i], tempBoard );
                moves.Add( newMove );
                tempBoard.MakeMove( newMove );
            }
        }

        moves ??= new();

        _chessEngine.ChangePosition( fen, moves );
        _chessEngine.DrawBoard();
    }

    private void HandleGoCommand( string[] args )
    {
        int depth = 100;
        int wTime = 0;
        int bTime = 0;
        int wInc = 0;
        int bInc = 0;
        int movesToGo = TimeManager.TimeDivider;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "depth":
                    depth = int.Parse( args[i + 1] );
                    break;
                case "wtime":
                    wTime = int.Parse( args[i + 1] );
                    break;
                case "btime":
                    bTime = int.Parse( args[i + 1] );
                    break;
                case "movetime":
                    wTime = int.Parse( args[i + 1] ) * TimeManager.TimeDivider;
                    bTime = int.Parse( args[i + 1] ) * TimeManager.TimeDivider;
                    break;
                case "winc":
                    wInc = int.Parse( args[i + 1] );
                    break;
                case "binc":
                    bInc = int.Parse( args[i + 1] );
                    break;
                case "movestogo":
                    movesToGo = int.Parse( args[i + 1] );
                    break;
            }
        }

        if(depth != 100)
        {
            wTime = int.MaxValue;
            bTime = int.MaxValue;
        }

        if (movesToGo == 0)
            movesToGo = TimeManager.TimeDivider;

        Thread searchThread = new (SearchThread);
        searchThread.Start( new SearchData( depth, wTime, bTime, wInc, bInc, movesToGo ) );
    }

    private void SearchThread( object? data )
    {
        SearchData searchData = (SearchData)data!;
        MoveData bestMove = _chessEngine.FindBestMove(searchData.Depth, searchData.WhiteTime, searchData.BlackTime, searchData.WhiteIncrement, searchData.BlackIncrement, searchData.MovesToGo);
        Console.WriteLine( $"bestmove {bestMove}" );
    }

    private record struct SearchData(int Depth, int WhiteTime, int BlackTime, int WhiteIncrement, int BlackIncrement, int MovesToGo );
}
using System;
using System.Diagnostics;
using System.Threading;
using Engine;
using Engine.Perft;

namespace Interface;

internal class GeneralCommandProcessor : CommandProcessor
{
    public GeneralCommandProcessor( ChessEngine chessEngine ) : base( chessEngine ) {  }

    public override void ProcessCommand( string[] commandSplit )
    {
        switch (commandSplit[0])
        {
            case "perft": 
                HandlePerftCommand( commandSplit[1..] );
                break;
            case "splitperft":
                HandleSplitPerftCommand( commandSplit[1..] );
                break;
            case "eval":
                HandleEvalCommand();
                break;
            case "stop":
                HandleInterruptCommand( commandSplit[1..] );
                break;
            case "quit":
                HandleQuitCommand( commandSplit[1..] );
                break;
        }
    }

    private void HandlePerftCommand( string[] args )
    {

        PerftData perftData = new()
        {
            Fen = "",
            Depth = int.Parse(args[0]),
            Divide = false
        };

        if (args.Length > 1)
            perftData.Fen = args[1] + ' ' + args[2] + ' ' + args[3] + ' ' + args[4] + ' ' + args[5] + ' ' + args[6];

        PerftThread(perftData);
    }

    private void HandleSplitPerftCommand( string[] args )
    {

        PerftData perftData = new()
        {
            Fen = "",
            Depth = int.Parse(args[0]),
            Divide = true
        };

        if (args.Length > 1)
            perftData.Fen = args[1] + ' ' + args[2] + ' ' + args[3] + ' ' + args[4] + ' ' + args[5] + ' ' + args[6];

        PerftThread(perftData);
    }

    private void HandleEvalCommand()
    {
        _chessEngine.DebugEval();
    }

    private void PerftThread( object? data )
    {
        Stopwatch stopwatch = new();
        stopwatch.Restart();

        PerftData perftData = (PerftData)data!;

        ulong perftResult = perftData.Fen == "" ? _chessEngine.Perft(perftData.Depth, perftData.Divide) : _chessEngine.Perft(perftData.Fen, perftData.Depth, perftData.Divide);
        double totalMicroseconds = stopwatch.Elapsed.TotalMicroseconds;

        ulong nps = (ulong)(perftResult * 1_000_000 / totalMicroseconds);

        if (PerftTest.CancellationToken)
            return;

        Console.WriteLine( $"Depth: {perftData.Depth}, Nodes: {perftResult}, Time: {totalMicroseconds / 1000}ms, Nps {nps}" );
    }

    private struct PerftData
    {
        public string Fen;
        public int Depth;
        public bool Divide;
    }

    private void HandleInterruptCommand( string[] args )
    {
        _chessEngine.InterruptLoops();
    }

    private void HandleQuitCommand( string[] args )
    {
        HandleInterruptCommand( args );
        Thread.Sleep( 100 );
        Environment.Exit( 0 );
    }
}
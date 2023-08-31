using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Engine.Board;
using Engine.Data.Enums;
using Engine.Move;

namespace Benchmarks.Systems;

[DisassemblyDiagnoser(maxDepth: 6)]
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class MoveBenchmarks
{
    private static readonly MoveData[] MoveBuffer = new MoveData[300];

    [Benchmark]
    public BoardData CreateMove()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        for (int i = 0; i < 100; i++)
        {
            Helpers.Use(new MoveData(board, 0, 0, PieceType.Queen, false, false, false, false));
        }

        return board;
    }

    [Benchmark]
    public BoardData ReadMove()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        MoveData move = new(board,0,0,PieceType.Queen,false,false,false,false);
        for (int i = 0; i < 100; i++)
        {
            Helpers.Use(move.FromSquareIndex);
            Helpers.Use(move.ToSquareIndex);

            Helpers.Use(move.MovingPieceType);
            Helpers.Use( move.TargetPieceType);
            Helpers.Use(move.PromotionPieceType);

            Helpers.Use(move.IsCapture);
            Helpers.Use(move.IsCastle);
            Helpers.Use(move.IsEnPassant);
            Helpers.Use(move.IsPromotion);
        }

        return board;
    }

    [Benchmark]
    public BoardData MakeMove()
    {
        BoardData board = BoardProvider.Create(BoardProvider.KiwipetePosition);
        MoveData move = new("f3h3", board);
        for (int i = 0; i < 100; i++)
        {
            Helpers.Use(board.MakeMove( move ));
            board.UnmakeMove();
        }

        return board;
    }

    [Benchmark]
    public BoardData GenerateMoves()
    {
        BoardData board = BoardProvider.Create(BoardProvider.KiwipetePosition);
        for (int i = 0; i < 100; i++)
        {
            MoveList moves = new(MoveBuffer );
            board.GenerateAllPseudoLegalMoves( ref moves );
        }

        return board;
    }

    [Benchmark]
    public BoardData GenerateTacticalMoves()
    {
        BoardData board = BoardProvider.Create(BoardProvider.KiwipetePosition);
        for (int i = 0; i < 100; i++)
        {
            MoveList moves = new(MoveBuffer );
            board.GenerateTacticalPseudoLegalMoves( ref moves );
        }

        return board;
    }
}
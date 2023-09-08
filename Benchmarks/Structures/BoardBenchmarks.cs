using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Engine.Board;
using Engine.Zobrist;

namespace Benchmarks.Structures;

[DisassemblyDiagnoser(maxDepth: 6)]
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class BoardBenchmarks
{
    [Benchmark]
    public unsafe void CreateBoard()
    {
        for (int i = 0; i < 100; i++)
        {
            BoardData data = BoardProvider.Create(BoardProvider.StartPosition);
            Helpers.Use(&data);
        }
    }

    [Benchmark]
    public BoardData IsKingInCheck()
    {
        BoardData board = new();
        for (int i = 0; i < 100; i++)
        {
            Helpers.Use(board.IsSideToMoveInCheck);
        }

        return board;
    }

    [Benchmark]
    public BoardData SetIsKingInCheck()
    {
        BoardData board = new();
        for (int i = 0; i < 100; i++)
        {
            board.IsSideToMoveInCheck = false;
        }

        return board;
    }

    [Benchmark]
    public BoardData SetCanWhiteCastleQueenSide()
    {
        BoardData board = new();
        for (int i = 0; i < 100; i++)
        {
            board.CanWhiteCastleQueenSide = false;
        }

        return board;
    }

    [Benchmark]
    public BoardData IsSquareAttacked()
    {
        BoardData board = new();
        for (int i = 0; i < 100; i++)
        {
            Helpers.Use(board.IsSquareAttacked( 44, 0 ));
        }

        return board;
    }

    [Benchmark]
    public BoardData GenerateZobristKey()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        for (int i = 0; i < 100; i++)
        {
            Helpers.Use(ZobristHashing.GenerateKey( board ));
        }

        return board;
    }

    [Benchmark]
    public BoardData ModifyZobristKey()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        BoardData prevBoard = BoardProvider.Create(BoardProvider.KiwipetePosition);
        for (int i = 0; i < 100; i++)
        {
            Helpers.Use(ZobristHashing.ModifyKey( board, prevBoard ));
        }

        return board;
    }
}

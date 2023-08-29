using BenchmarkDotNet.Attributes;
using Engine.Board;
using Engine.Zobrist;

namespace Benchmarks.Structures;

[DisassemblyDiagnoser]
public class BoardBenchmarks
{
    [Benchmark]
    public void CreateBoard()
    {
        for (int i = 0; i < 1000000; i++)
        {
            BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        }
    }

    [Benchmark]
    public void IsKingInCheck()
    {
        BoardData board = new();
        for (int i = 0; i < 100000000; i++)
        {
            var x =  board.IsBlackKingInCheck;
        }
    }

    [Benchmark]
    public void SetIsKingInCheck()
    {
        BoardData board = new();
        for (int i = 0; i < 100000000; i++)
        {
            board.IsBlackKingInCheck = 0;
        }
    }

    [Benchmark]
    public void SetCanWhiteCastleQueenSide()
    {
        BoardData board = new();
        for (int i = 0; i < 100000000; i++)
        {
            board.CanWhiteCastleQueenSide = 0;
        }
    }

    [Benchmark]
    public void IsSquareAttacked()
    {
        BoardData board = new();
        for (int i = 0; i < 100000000; i++)
        {
            board.IsSquareAttacked( 44, 0 );
        }
    }

    [Benchmark]
    public void GenerateZobristKey()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        for (int i = 0; i < 100000000; i++)
        {
            ZobristHashing.GenerateKey( board );
        }
    }

    [Benchmark]
    public void ModifyZobristKey()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        BoardData prevBoard = BoardProvider.Create(BoardProvider.KiwipetePosition);
        for (int i = 0; i < 100000000; i++)
        {
            ZobristHashing.ModifyKey( board, prevBoard );
        }
    }
}

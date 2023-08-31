using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Engine.Board;
using Engine.Search;

namespace Benchmarks.Search;

[DisassemblyDiagnoser( maxDepth: 6 )]
[MemoryDiagnoser]
[SimpleJob( RuntimeMoniker.Net80 )]
public class SearchBenchmarks
{
    [Benchmark]
    public SearchSystem DepthOneSearch()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        SearchSystem search = new();
        for (int i = 0; i < 100; i++)
        {
            search.FindBestMove( new SearchParameters( board, 1 ) );
        }
        return search;
    }

    [Benchmark]
    public SearchSystem DepthTwoSearch()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        SearchSystem search = new();
        for (int i = 0; i < 100; i++)
        {
            search.FindBestMove( new SearchParameters( board, 2 ) );
        }
        return search;
    }

    [Benchmark]
    public SearchSystem DepthThreeSearch()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        SearchSystem search = new();
        for (int i = 0; i < 100; i++)
        {
            search.FindBestMove(new SearchParameters(board, 3));
        }
        return search;
    }

    [Benchmark]
    public SearchSystem DepthFourSearch()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        SearchSystem search = new();
        for (int i = 0; i < 100; i++)
        {
            search.FindBestMove( new SearchParameters( board, 4 ) );
        }
        return search;
    }

    [Benchmark]
    public SearchSystem DepthFiveSearch()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        SearchSystem search = new();
        for (int i = 0; i < 100; i++)
        {
            search.FindBestMove( new SearchParameters( board, 5 ) );
        }
        return search;
    }
}

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
    public SearchSystem DepthFiveSearch()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        SearchSystem search = new();
        for (int i = 0; i < 10; i++)
        {
            search.FindBestMove( new SearchParameters( board, 5 ) );
        }
        return search;
    }
}

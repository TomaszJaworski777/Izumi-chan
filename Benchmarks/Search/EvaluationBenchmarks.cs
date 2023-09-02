using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Engine.Board;
using Engine.Evaluation;

namespace Benchmarks.Search;

[DisassemblyDiagnoser( maxDepth: 6 )]
[MemoryDiagnoser]
[SimpleJob( RuntimeMoniker.Net80 )]
public class EvaluationBenchmarks
{
    [Benchmark]
    public BoardData EvaluatePosition()
    {
        BoardData board = BoardProvider.Create(BoardProvider.StartPosition);
        for (int i = 0; i < 100; i++)
        {
            int score = EvaluationSystem.EvaluatePosition(ref board);
            Helpers.Use( score );
        }
        return board;
    }
}

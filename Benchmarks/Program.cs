using BenchmarkDotNet.Running;
using Benchmarks.Data;
using Benchmarks.Search;
using Benchmarks.Structures;
using Benchmarks.Systems;

namespace Benchmarks
{
    internal class Program
    {
        private static void Main( string[] args )
        {
            BenchmarkRunner.Run<BoardBenchmarks>();
            BenchmarkRunner.Run<BitboardBenchmarks>();
            BenchmarkRunner.Run<MoveBenchmarks>();
            BenchmarkRunner.Run<SearchBenchmarks>();
            BenchmarkRunner.Run<EvaluationBenchmarks>();
        }
    }
}

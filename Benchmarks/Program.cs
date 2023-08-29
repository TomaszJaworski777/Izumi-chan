using BenchmarkDotNet.Running;
using Benchmarks.Data;
using Benchmarks.Structures;

namespace Benchmarks
{
    internal class Program
    {
        private static void Main( string[] args )
        {
            BenchmarkRunner.Run<BoardBenchmarks>();
            BenchmarkRunner.Run<BitboardBenchmarks>();
        }
    }
}

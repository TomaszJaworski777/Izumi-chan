using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Engine.Data.Bitboards;

namespace Benchmarks.Data;

[DisassemblyDiagnoser(maxDepth: 6)]
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.NativeAot80)]
public class BitboardBenchmarks
{
    [Benchmark]
    public Bitboard GetBitValue()
    {
        Bitboard bitboard = 666;
        for (int i = 0; i < 100000000; i++)
        {
            Helpers.Use(bitboard.GetBitValue( 63 ));
        }
        return bitboard;
    }

    [Benchmark]
    public Bitboard SetBitValue()
    {
        Bitboard bitboard = 666;
        for (int i = 0; i < 100000000; i++)
        {
            bitboard.SetBitToOne( 63 );
        }
        return bitboard;
    }

    [Benchmark]
    public Bitboard GetValueChunk()
    {
        Bitboard bitboard = 666;
        for (int i = 0; i < 100000000; i++)
        {
            Helpers.Use(bitboard.GetValueChunk( 30, 127 ));
        }
        return bitboard;
    }

    [Benchmark]
    public Bitboard SetValueChunk()
    {
        Bitboard bitboard = 666;
        for (int i = 0; i < 100000000; i++)
        {
            bitboard.SetValueChunk( 30, 127,  69 );
        }

        return bitboard;
    }
}

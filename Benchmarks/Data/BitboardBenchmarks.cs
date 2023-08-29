using BenchmarkDotNet.Attributes;
using Engine.Data.Bitboards;

namespace Benchmarks.Data;

[DisassemblyDiagnoser]
public class BitboardBenchmarks
{
    [Benchmark]
    public void GetBitValue()
    {
        Bitboard bitboard = 666;
        for (int i = 0; i < 100000000; i++)
        {
            bitboard.GetBitValue( 63 );
        }
    }

    [Benchmark]
    public void SetBitValue()
    {
        Bitboard bitboard = 666;
        for (int i = 0; i < 100000000; i++)
        {
            bitboard.SetBitToOne( 63 );
        }
    }

    [Benchmark]
    public void GetValueChunk()
    {
        Bitboard bitboard = 666;
        for (int i = 0; i < 100000000; i++)
        {
            bitboard.GetValueChunk( 30, 127 );
        }
    }

    [Benchmark]
    public void SetValueChunk()
    {
        Bitboard bitboard = 666;
        for (int i = 0; i < 100000000; i++)
        {
            bitboard.SetValueChunk( 30, 127,  69 );
        }
    }
}

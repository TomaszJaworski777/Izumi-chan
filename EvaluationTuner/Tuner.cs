using Engine.Board;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace EvaluationTuner;

internal static class Tuner
{
    private static readonly TrainingEntry[] _trainingEntries;

    static Tuner()
    {
        ReadOnlySpan<string> testValues = File.ReadAllLines(@"../../../train_positions_eval.epd", Encoding.UTF8);
        _trainingEntries = new TrainingEntry[testValues.Length];

        for (int i = 0; i < testValues.Length; i++)
        {
            ReadOnlySpan<string> testSplit = testValues[i].Split(' ');

            _trainingEntries[i] = new TrainingEntry
            {
                Board = BoardProvider.Create( testSplit[0] + ' ' + testSplit[1] + ' ' + testSplit[2] + ' ' + testSplit[3] + " 0 1" ),
                Result = Sigmoid( int.Parse( testSplit[4] ) )
            };
        }

        Console.WriteLine( "Tuner initialized!" );
    }

    [StructLayout( LayoutKind.Sequential )]
    private struct TrainingEntry
    {
        public BoardData Board;
        public double Result;
    }

    private static ulong totalError;
    private static int iteratedCount;
    public static double TestValues( ModifiableEvaluationSheet sheet, double kFactor )
    {
        totalError = 0;
        iteratedCount = 0;
        int dataLength = _trainingEntries.Length / 10;

        for (int i = 0; i < 10; i++)
        {
            int startIndex = i * dataLength;
            int endIndex = (i + 1) * dataLength;

            if (i == 9)
                endIndex = _trainingEntries.Length;

            ThreadPool.QueueUserWorkItem( TestValueThread, new TestValueData
            {
                StartIndex = startIndex,
                EndIndex = endIndex,
                Sheet = sheet,
                KFactor = kFactor
            } );
        }

        while (_trainingEntries.Length != iteratedCount)
        {
            Thread.Sleep( 1 );
        }

        double error = totalError / 1_000_000_000d;
        return (error / _trainingEntries.Length) * 1000;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static double Sigmoid( double x ) => 1 / (1 + Math.Pow( Math.E, -x / 222 ));

    private struct TestValueData
    {
        public int StartIndex;
        public int EndIndex;
        public ModifiableEvaluationSheet Sheet;
        public double KFactor;
    }

    private static void TestValueThread( object? state )
    {
        TestValueData data = (TestValueData)state!;
        foreach (var test in _trainingEntries.AsSpan()[data.StartIndex..data.EndIndex])
        {
            int evaluation = InjectableEvaluation.EvaluatePosition(test.Board, data.Sheet) * (test.Board.SideToMove == 0 ? 1 : -1);
            ;
            Interlocked.Add( ref totalError, (ulong)(Math.Pow( test.Result - Sigmoid( evaluation * data.KFactor ), 2 ) * 1_000_000_000) );
            Interlocked.Increment( ref iteratedCount );
        }
    }
}

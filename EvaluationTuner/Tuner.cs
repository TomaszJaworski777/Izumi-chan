using Engine.Board;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace EvaluationTuner;

internal static class Tuner
{
    private static readonly TrainingEntry[] trainingEntries;

    static Tuner()
    {
        ReadOnlySpan<string> testValues = File.ReadAllLines(@"../../../train_positions_eval.epd", Encoding.UTF8);
        trainingEntries = new TrainingEntry[testValues.Length];

        for (int i = 0; i < testValues.Length; i++)
        {
            ReadOnlySpan<string> testSplit = testValues[i].Split(' ');

            trainingEntries[i] = new TrainingEntry {
                Board = BoardProvider.Create( testSplit[0] + ' ' + testSplit[1] + ' ' + testSplit[2] + ' ' + testSplit[3] + " 0 1" ),
                Result = Sigmoid( int.Parse( testSplit[4] ) )
            };
        }

        Console.WriteLine( "Tuner initialized!" );
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct TrainingEntry
    {
        public BoardData Board;
        public double Result;
    }

    public static double TestValues( ModifiableEvaluationSheet sheet, double kFactor )
    {
        double totalError = 0;

        foreach (var test in trainingEntries.AsSpan())
        {
            int evaluation = InjectableEvaluation.EvaluatePosition(test.Board, sheet) * (test.Board.SideToMove == 0 ? 1 : -1);
            totalError += Math.Pow( test.Result - Sigmoid( evaluation * kFactor ), 2 );
        }

        return (totalError / trainingEntries.Length) * 1000;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double Sigmoid( double x ) => 1 / (1 + Math.Pow( Math.E, -x/222 ));
}

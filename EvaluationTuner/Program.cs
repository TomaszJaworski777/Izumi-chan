using Engine;
using System.Numerics;

namespace EvaluationTuner;

internal class Program
{
    private static bool _cancelationToken = false;

    static void Main( string[] args )
    {
        Console.WriteLine( $"Evaluation Tuner v{EngineCredentials.Version}\n" );
        ModifiableEvaluationSheet sheet = new();

        double kFactor = CalculateKFactor(sheet);

        Console.WriteLine( $"K-Factor: {kFactor}\n" );

        Thread thread = new Thread(CancelationListener);
        thread.Start();

        while (true)
        {
            sheet = AdjustEvalSheet( sheet, kFactor );

            sheet.Capture();

            if (_cancelationToken)
                break;
        }
    }

    private static double CalculateKFactor( ModifiableEvaluationSheet sheet )
    {
        Console.WriteLine( "Calculating K-factor.." );

        double result = 2;
        double currentError = Tuner.TestValues( sheet, result );
        Console.WriteLine( "base set" );

        for (int i = 0; i < 15; i++)
        {
            double upperError = Tuner.TestValues( sheet, result + (1f / Math.Pow( 2, i )) );
            double lowerError = Tuner.TestValues( sheet, result - (1f / Math.Pow( 2, i )) );

            if (upperError < currentError && upperError <= lowerError)
            {
                result += 1f / Math.Pow( 2, i );
                currentError = upperError;
            } else if (lowerError < currentError)
            {
                result -= 1f / Math.Pow( 2, i );
                currentError = lowerError;
            }

            Console.WriteLine( $"Current factor: {result}" );
        }

        return result;
    }

    private static ModifiableEvaluationSheet AdjustEvalSheet( ModifiableEvaluationSheet sheet, double kFactor )
    {
        Console.WriteLine( "Starting next iteration..." );

        double currentError = Tuner.TestValues( sheet, kFactor );

        //piece values
        for (int i = 0; i < sheet.PieceValues.Length - 1; i++)
        {
            if (_cancelationToken)
                break;

            Console.WriteLine( $"Piece Values {i}/{sheet.PieceValues.Length-1}" );
            ValueTweaker.TweakValueUShort( ref sheet.PieceValues[i], ref sheet, kFactor, ref currentError );
        }

        //psts tables
        for (int i = 16; i < sheet.PstsTable.Length; i++)
        {
            if (_cancelationToken)
                break;

            Console.WriteLine( $"PSTS Values {i}/{sheet.PstsTable.Length}" );
            ValueTweaker.TweakValueSByte( ref sheet.PstsTable[i], ref sheet, kFactor, ref currentError );
        }

        //TempoBonus adjustment
        Console.WriteLine( $"TempoBonus" );
        ValueTweaker.TweakValueByte( ref sheet.TempoBonus, ref sheet, kFactor, ref currentError );

        //BishopPairMidgameBonus adjustment
        Console.WriteLine( $"BishopPairMidgameBonus" );
        ValueTweaker.TweakValueUShort( ref sheet.BishopPairMidgameBonus, ref sheet, kFactor, ref currentError );

        //BishopPairEndgameBonus adjustment
        Console.WriteLine( $"BishopPairEndgameBonus" );
        ValueTweaker.TweakValueUShort( ref sheet.BishopPairEndgameBonus, ref sheet, kFactor, ref currentError );

        //DoublePawnMidgamePenalty adjustment
        Console.WriteLine( $"DoublePawnMidgamePenalty" );
        ValueTweaker.TweakValueInt( ref sheet.DoublePawnMidgamePenalty, ref sheet, kFactor, ref currentError );

        //DoublePawnEndgamePenalty adjustment
        Console.WriteLine( $"DoublePawnEndgamePenalty" );
        ValueTweaker.TweakValueInt( ref sheet.DoublePawnEndgamePenalty, ref sheet, kFactor, ref currentError );

        return sheet;
    }

    private static void CancelationListener()
    {
        while (true)
        {
            string message = Console.ReadLine()!;
            if (message is "s" or "stop" or "q" or "quit")
            {
                _cancelationToken = true;
                break;
            }
        }
    }
}

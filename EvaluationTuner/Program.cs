using Engine;

namespace EvaluationTuner;

internal class Program
{
    private static bool _cancelationToken = false;

    static void Main( string[] args )
    {
        Console.WriteLine( $"Evaluation Tuner v{EngineCredentials.Version}\n" );

        double kFactor = CalculateKFactor();

        Console.WriteLine( $"K-Factor: {kFactor}\n" );

        Thread thread = new Thread(CancelationListener);
        thread.Start();

        ModifiableEvaluationSheet sheet = new();
        while (true)
        {
            sheet = AdjustEvalSheet( sheet, kFactor );

            if (_cancelationToken)
                break;
        }

        sheet.Capture();
    }

    private static double CalculateKFactor()
    {
        Console.WriteLine( "Calculating K-factor.." );

        double result = 2;
        ModifiableEvaluationSheet sheet = new();
        double currentError = Tuner.TestValues( sheet, result );

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

        //DoublePawnMidgamePenalty adjustment
        Console.WriteLine( $"DoublePawnMidgamePenalty" );

        sheet.DoublePawnMidgamePenalty += 1;
        double upperError = Tuner.TestValues( sheet, kFactor );
        sheet.DoublePawnMidgamePenalty -= 2;
        double lowerError = Tuner.TestValues( sheet, kFactor );
        sheet.DoublePawnMidgamePenalty += 1;

        if (upperError < currentError && upperError <= lowerError)
        {
            sheet.DoublePawnMidgamePenalty += 1;
            currentError = upperError;
            Console.WriteLine( $"   New error: {currentError}" );
        } else if (lowerError < currentError)
        {
            sheet.DoublePawnMidgamePenalty -= 1;
            currentError = lowerError;
            Console.WriteLine( $"   New error: {currentError}" );
        }

        //DoublePawnEndgamePenalty adjustment
        Console.WriteLine( $"DoublePawnEndgamePenalty" );

        sheet.DoublePawnEndgamePenalty += 1;
        upperError = Tuner.TestValues( sheet, kFactor );
        sheet.DoublePawnEndgamePenalty -= 2;
        lowerError = Tuner.TestValues( sheet, kFactor );
        sheet.DoublePawnEndgamePenalty += 1;

        if (upperError < currentError && upperError <= lowerError)
        {
            sheet.DoublePawnEndgamePenalty += 1;
            currentError = upperError;
            Console.WriteLine( $"   New error: {currentError}" );
        } else if (lowerError < currentError)
        {
            sheet.DoublePawnEndgamePenalty -= 1;
            currentError = lowerError;
            Console.WriteLine( $"   New error: {currentError}" );
        }

        //piece values
        for (int i = 0; i < sheet.PieceValues.Length - 1; i++)
        {
            if (_cancelationToken)
                break;

            Console.WriteLine( $"Piece Values {i}/{sheet.PieceValues.Length}" );

            sheet.PieceValues[i] += 1;
            upperError = Tuner.TestValues( sheet, kFactor );
            sheet.PieceValues[i] -= 2;
            lowerError = Tuner.TestValues( sheet, kFactor );
            sheet.PieceValues[i] += 1;

            if (upperError < currentError && upperError <= lowerError)
            {
                sheet.PieceValues[i] += 1;
                currentError = upperError;
                Console.WriteLine( $"   New error: {currentError}" );
            } else if (lowerError < currentError)
            {
                sheet.PieceValues[i] -= 1;
                currentError = lowerError;
                Console.WriteLine( $"   New error: {currentError}" );
            }
        }

        //psts tables
        for (int i = 16; i < sheet.PstsTable.Length; i++)
        {
            if (_cancelationToken)
                break;

            Console.WriteLine( $"PSTS Values {i}/{sheet.PstsTable.Length}" );

            sheet.PstsTable[i] += 1;
            upperError = Tuner.TestValues( sheet, kFactor );
            sheet.PstsTable[i] -= 2;
            lowerError = Tuner.TestValues( sheet, kFactor );
            sheet.PstsTable[i] += 1;

            if (upperError < currentError && upperError <= lowerError)
            {
                sheet.PstsTable[i] += 1;
                currentError = upperError;
                Console.WriteLine( $"   New error: {currentError}" );
            } else if (lowerError < currentError)
            {
                sheet.PstsTable[i] -= 1;
                currentError = lowerError;
                Console.WriteLine( $"   New error: {currentError}" );
            }
        }

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

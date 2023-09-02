namespace EvaluationTuner;

internal static class ValueTweaker
{
    public static void TweakValueInt( ref int value, ref ModifiableEvaluationSheet sheet, double kFactor, ref double currentError )
    {
        for (int i = 1; i <= 6; i++)
        {
            int change = 10/i;

            value += change;
            double upperError = Tuner.TestValues( sheet, kFactor );
            value -= 2 * change;
            double lowerError = Tuner.TestValues( sheet, kFactor );
            value += change;

            if (upperError < currentError && upperError <= lowerError)
            {
                value += change;
                currentError = upperError;
                Console.WriteLine( $"   New error: {currentError}" );
            } else if (lowerError < currentError)
            {
                value -= change;
                currentError = lowerError;
                Console.WriteLine( $"   New error: {currentError}" );
            }
        }
    }

    public static void TweakValueUShort( ref ushort value, ref ModifiableEvaluationSheet sheet, double kFactor, ref double currentError )
    {
        for (int i = 1; i <= 6; i++)
        {
            ushort change = (ushort)(10/i);

            value += change;
            double upperError = Tuner.TestValues( sheet, kFactor );
            value -= (ushort)(2 * change);
            double lowerError = Tuner.TestValues( sheet, kFactor );
            value += change;

            if (upperError < currentError && upperError <= lowerError)
            {
                value += change;
                currentError = upperError;
                Console.WriteLine( $"   New error: {currentError}" );
            } else if (lowerError < currentError)
            {
                value -= change;
                currentError = lowerError;
                Console.WriteLine( $"   New error: {currentError}" );
            }
        }
    }

    public static void TweakValueSByte( ref sbyte value, ref ModifiableEvaluationSheet sheet, double kFactor, ref double currentError )
    {
        for (int i = 1; i <= 6; i++)
        {
            sbyte change = (sbyte)(10/i);

            value += change;
            double upperError = Tuner.TestValues( sheet, kFactor );
            value -= (sbyte)(2 * change);
            double lowerError = Tuner.TestValues( sheet, kFactor );
            value += change;

            if (upperError < currentError && upperError <= lowerError)
            {
                value += change;
                currentError = upperError;
                Console.WriteLine( $"   New error: {currentError}" );
            } else if (lowerError < currentError)
            {
                value -= change;
                currentError = lowerError;
                Console.WriteLine( $"   New error: {currentError}" );
            }
        }
    }
}

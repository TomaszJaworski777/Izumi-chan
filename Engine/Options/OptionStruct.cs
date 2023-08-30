namespace Engine.Options;

public readonly struct OptionStruct
{
    public OptionValueType Type { get; }
    public object Value { get; init; }
    public int MinValue { get; }
    public int MaxValue { get; }

    public OptionStruct( OptionValueType type, object defaultValue)
    {
        Type = type;
        Value = defaultValue;
    }

    public OptionStruct( OptionValueType type, int defaultValue, int min = 0, int max = 0 )
    {
        Type = type;

        MinValue = min;
        MaxValue = max;

        Value = defaultValue;
    }

    public static explicit operator int( OptionStruct option ) => (int)option.Value;
    public static explicit operator bool( OptionStruct option ) => (bool)option.Value;
    public static explicit operator string( OptionStruct option ) => (string)option.Value;
}

namespace Engine.Options;

public struct OptionStruct
{
    public OptionValueType Type { get; private set; }
    public object Value { get; private set; }
    public int MinValue { get; private set; }
    public int MaxValue { get; private set; }

    public OptionStruct( OptionValueType type, object defaultValue, int min = 0, int max = 0 )
    {
        Type = type;

        if (type != OptionValueType.Spin && (min | max) > 0)
            Console.WriteLine( "Incorrect option definition!" );

        MinValue = min;
        MaxValue = max;

        Value = defaultValue;
    }

    public void ChangeOption( object newValue )
    {
        switch (Type)
        {
            case OptionValueType.Spin:
                if(newValue is not int)
                {
                    Console.WriteLine( "Incorrect option change!" );
                    return;
                }

                int value = (int)newValue;

                if ( value < MinValue || value > MaxValue)
                {
                    Console.WriteLine( "Incorrect option change!" );
                    return;
                }

                break;
            case OptionValueType.Check:
                if (newValue is not bool)
                {
                    Console.WriteLine( "Incorrect option change!" );
                    return;
                }
                break;
            case OptionValueType.String:
                if (newValue is not string)
                {
                    Console.WriteLine( "Incorrect option change!" );
                    return;
                }
                break;
        }

        Value = newValue;
    }

    public static implicit operator int( OptionStruct option ) => (int)option.Value;
    public static implicit operator bool( OptionStruct option ) => (bool)option.Value;
    public static implicit operator string( OptionStruct option ) => (string)option.Value;
}

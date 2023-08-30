namespace Engine.Options;

public static class EngineOptions
{
    public const string MoveOverheadKey = "MOVEOVERHEAD";

    private static readonly Dictionary<string, OptionStruct> _options = new();

    public static Dictionary<string, OptionStruct> Options => _options;

    static EngineOptions()
    {
        AddOption( MoveOverheadKey, new OptionStruct( OptionValueType.Spin, 0, 0, 5000 ) );
    }

    public static void AddOption(string name, OptionStruct option )
    {
        _options.TryAdd( name.ToUpper(), option );
    }

    public static OptionStruct GetOption( string name ) => _options[name.ToUpper()];

    public static void ChangeOption( string name, string newValue )
    {
        if (!_options.TryGetValue( name.ToUpper(), out OptionStruct value ))
        {
            Console.WriteLine( $"Option `{name.ToUpper()}` doesn't exist!" );
            return;
        }

        switch (value.Type)
        {
            case OptionValueType.Spin:
                ChangeOption( name, int.Parse(newValue) );
                break;
            case OptionValueType.Check:
                ChangeOption( name, newValue == "true" );
                break;
            case OptionValueType.String:
                ChangeOption( name, newValue );
                break;
        }
    }

    public static void ChangeOption( string name, object newValue )
    {
        if (!_options.TryGetValue( name.ToUpper(), out OptionStruct value ))
        {
            Console.WriteLine( $"Option `{name.ToUpper()}` doesn't exist!" );
            return;
        }

        value.ChangeOption( newValue );
        _options[name.ToUpper()] = value;
    }
}

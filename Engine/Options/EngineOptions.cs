using System;
using System.Collections.Generic;

namespace Engine.Options;

public static class EngineOptions
{
    public const string MoveOverheadKey = "MoveOverhead";

    private static readonly Dictionary<string, OptionStruct> Options = new(StringComparer.OrdinalIgnoreCase);

    public static IEnumerable<KeyValuePair<string, OptionStruct>> AllValues => Options;

    static EngineOptions()
    {
        AddOption( MoveOverheadKey, new OptionStruct( OptionValueType.Spin, 10, 0, 5000 ) );
    }

    public static void AddOption(string name, OptionStruct option )
    {
        Options.Add( name, option );
    }

    public static OptionStruct GetOption( string name ) => Options[name];

    public static void ChangeOption( string name, string newValue )
    {
        if (!Options.TryGetValue( name, out OptionStruct value ))
        {
            throw new InvalidOperationException( $"Option `{name.ToUpperInvariant()}` doesn't exist!" );
        }

        switch (value.Type)
        {
            case OptionValueType.Spin:
                ChangeOption( name, int.Parse(newValue) );
                break;
            case OptionValueType.Check:
                ChangeOption( name, bool.Parse(newValue) );
                break;
            case OptionValueType.String:
                ChangeOption( name, (object)newValue );
                break;
        }
    }

    public static void ChangeOption( string name, object newValue )
    {
        if (!Options.TryGetValue( name, out OptionStruct value ))
        {
            throw new InvalidOperationException( $"Option `{name.ToUpperInvariant()}` doesn't exist!" );
        }

        if (value.Value.GetType() != newValue.GetType())
        {
            throw new ArgumentException("Invalid type", nameof(newValue));
        }

        Options[name] = value with { Value = newValue };
    }
}

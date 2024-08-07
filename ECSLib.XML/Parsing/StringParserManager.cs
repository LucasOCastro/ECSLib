using System.Globalization;

namespace ECSLib.XML.Parsing;

public static class StringParserManager
{
    private static readonly Dictionary<Type, IStringParser> Parsers = [];

    public static void AddParser(Type type, IStringParser parser) => Parsers[type] = parser;
    
    public static object? Parse(string str, Type type)
    {
        var parser = TryGetParserForType(type);
        return parser != null ? parser.Parse(str) : Convert.ChangeType(str, type, CultureInfo.InvariantCulture);
    }
    
    private static IStringParser? TryGetParserForType(Type? type)
    {
        while (type != null)
        {
            if (Parsers.TryGetValue(type, out var result)) 
                return result;
            if (type.IsConstructedGenericType && Parsers.TryGetValue(type.GetGenericTypeDefinition(), out result))
                return result;

            type = type.BaseType;
        }

        return null;
    }
}
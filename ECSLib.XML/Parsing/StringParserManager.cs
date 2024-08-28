
namespace ECSLib.XML.Parsing;

public static class StringParserManager
{
    private static readonly Dictionary<Type, ConstructorParserDelegate> Parsers = new()
    {
        [typeof(Nullable<>)] = ParseNullable
    }; 
    
    public static void AddParser(Type type, ConstructorParserDelegate parser) => Parsers[type] = parser;
    
    public static ConstructorParserDelegate? TryGetConstructorParserForType(Type? type)
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
    
    private static ParsedConstructor ParseNullable(string str, Type type)
    {
        if (!type.IsConstructedGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>))
            throw new ArgumentException($"Type should be {typeof(Nullable<>).Name}.", nameof(type));

        if (string.IsNullOrWhiteSpace(str) || str.Equals("null", StringComparison.InvariantCultureIgnoreCase))
            return new();

        var innerType = type.GenericTypeArguments[0];
        return new(type.GetConstructor([innerType])!, [str]); 
    }
}
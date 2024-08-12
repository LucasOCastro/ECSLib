
namespace ECSLib.XML.Parsing;

public static class StringParserManager
{
    private static readonly Dictionary<Type, IConstructorParser> Parsers = new()
    {
        [typeof(Nullable<>)] = new NullableStringParser()
    }; 
    
    public static void AddParser(Type type, IConstructorParser parser) => Parsers[type] = parser;
    
    public static IConstructorParser? TryGetConstructorParserForType(Type? type)
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
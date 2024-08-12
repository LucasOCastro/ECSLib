using System.Reflection;

namespace ECSLib.XML.Parsing;

public class NullableStringParser : IConstructorParser
{
    public (ConstructorInfo constructor, string[] args, (string name, string value)[] fields) Parse(string str, Type type)
    {
        if (!type.IsConstructedGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>))
            throw new ArgumentException($"Type should be {typeof(Nullable<>).Name}.", nameof(type));
        
        if (string.IsNullOrWhiteSpace(str) || str.Equals("null", StringComparison.InvariantCultureIgnoreCase))
            return (type.GetConstructor(Type.EmptyTypes)!, [], []);

        var innerType = type.GenericTypeArguments[0];
        return (type.GetConstructor([innerType])!, [str], []); 
    }
    
    /*public object? Parse(string str, Type type)
    {
        if (!type.IsConstructedGenericType || type.GetGenericTypeDefinition() != typeof(Nullable<>))
            throw new ArgumentException("Type should be Nullable<>.", nameof(type));

        if (string.IsNullOrWhiteSpace(str) || str.Equals("null", StringComparison.InvariantCultureIgnoreCase))
            return Activator.CreateInstance(type);

        var innerType = type.GenericTypeArguments[0];
        var innerValue = StringParserManager.Parse(str, innerType);
        return innerValue != null ? Activator.CreateInstance(type, innerValue) : Activator.CreateInstance(type);

    }*/
}
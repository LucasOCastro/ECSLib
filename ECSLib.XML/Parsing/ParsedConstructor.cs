using System.Reflection;

namespace ECSLib.XML.Parsing;

public readonly struct ParsedConstructor
{
    /// <summary>
    /// If null, the default empty constructor will be used.
    /// </summary>
    public readonly ConstructorInfo? Constructor;
    
    public readonly string[] ConstructorArgs = [];
    
    public readonly KeyValuePair<string, string>[] Fields = [];

    /// <summary>
    /// Uses the provided constructor with the provided parameters.
    /// </summary>
    public ParsedConstructor(ConstructorInfo constructor, string[] args)
    {
        Constructor = constructor;
        ConstructorArgs = args;
        Fields = [];
    }

    /// <summary>
    /// Uses the provided constructor with the provided parameters and fills the provided fields.
    /// </summary>
    public ParsedConstructor(ConstructorInfo constructor, string[] args, KeyValuePair<string, string>[] fields)
    {
        Constructor = constructor;
        ConstructorArgs = args;
        Fields = fields;
    }

    /// <summary>
    /// Uses the default constructor and fills the provided fields.
    /// </summary>
    public ParsedConstructor(KeyValuePair<string, string>[] fields)
    {
        Fields = fields;
    }

    /// <summary>
    /// Uses the default constructor.
    /// </summary>
    public ParsedConstructor()
    {
    }
}
namespace ECSLib.XML.Parsing;

public interface IConstructorParser
{
    ParsedConstructor Parse(string str, Type type);
}
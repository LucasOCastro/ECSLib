using System.Reflection;

namespace ECSLib.XML.Parsing;

public interface IConstructorParser
{
    (ConstructorInfo constructor, string[] args, (string name, string value)[] fields) Parse(string str, Type type);
}
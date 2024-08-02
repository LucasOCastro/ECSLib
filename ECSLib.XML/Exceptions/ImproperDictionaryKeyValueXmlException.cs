using System.Xml;

namespace ECSLib.XML.Exceptions;

public class ImproperDictionaryKeyValueXmlException(XmlNode dictionaryNode)
    : Exception($"XML Node {dictionaryNode.Name} in {dictionaryNode.OwnerDocument?.Name} is a dictionary and requires both <key> and <value> child nodes.");
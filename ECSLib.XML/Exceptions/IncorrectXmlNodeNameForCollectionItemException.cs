using System.Xml;

namespace ECSLib.XML.Exceptions;

public class IncorrectXmlNodeNameForCollectionItemException(XmlNode node) 
    : Exception($"XML Node {node.Name} in {node.OwnerDocument?.Name} must be <li> because the field is a collection.");
    
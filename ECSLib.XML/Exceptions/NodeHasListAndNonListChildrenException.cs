using System.Xml;

namespace ECSLib.XML.Exceptions;

public class NodeHasListAndNonListChildrenException(XmlNode node) 
    : Exception($"XML Node {node.Name} in {node.OwnerDocument?.Name} has a <li> child as well as non-list-item children.");
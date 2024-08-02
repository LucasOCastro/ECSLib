using System.Xml;

namespace ECSLib.XML.ValueEmitters;

internal static class ValueEmitterUtility
{
    public static IValueEmitter GetEmitterForNode(XmlNode fieldNode)
    {
        if (fieldNode is XmlElement {IsEmpty: true})
            return new EmptyValueEmitter();
        
        if (fieldNode.ChildNodes is [{ NodeType: XmlNodeType.Text }])
            return new TextValueEmitter(fieldNode.ChildNodes[0]?.Value ?? "");

        return new MultipleChildrenEmitter(fieldNode.ChildNodes.OfType<XmlElement>().ToList());
    }
}
using System.Xml;

namespace ECSLib.XML;

internal class EntityModel
{
    private const string ParentAttributeName = "Parent";
    private const string ParentAttributeSeparator = ";;";
    
    public string Name { get; }
    public Dictionary<string, Dictionary<string, string>> Components { get; } = [];

    private void SetField(string componentType, string field, string value)
    {
        if (!Components.TryGetValue(componentType, out var fields))
        {
            fields = new();
            Components.Add(componentType, fields);
        }
        fields[field] = value;
    }

    private void CopyComponentsFrom(Dictionary<string, Dictionary<string, string>> components)
    {
        foreach (var (type, fromFields) in components)
        foreach (var (field, value) in fromFields)
            SetField(type, field, value);
    }
    
    public EntityModel(XmlNode entityNode, ModelCache cache)
    {
        Name = entityNode.Name;
        
        if (entityNode.Attributes?[ParentAttributeName] is {} attribute)
        {
            var parents = attribute.Value.Split(ParentAttributeSeparator);
            foreach (var parent in parents.Select(cache.Request))
            {
                CopyComponentsFrom(parent.Components);
            }
        }
        
        foreach (XmlNode componentNode in entityNode.ChildNodes)
        {
            if (componentNode.NodeType != XmlNodeType.Element) continue;

            foreach (XmlNode fieldNode in componentNode.ChildNodes)
            {
                if (fieldNode.NodeType != XmlNodeType.Element) continue;
                if (fieldNode.Value == null) continue;
                SetField(componentNode.Name, fieldNode.Name, fieldNode.Value);
            }
        }
    }
}
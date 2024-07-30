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
    
    public EntityModel(XmlNode entityNode, ModelCache cache, TravelLog traveledModels)
    {
        Name = entityNode.Name;
        
        if (entityNode.Attributes?[ParentAttributeName] is {} attribute)
        {
            var parents = attribute.Value.Split(ParentAttributeSeparator);
            foreach (var parentName in parents)
            {
                //TODO current loop detection will throw if two parents inherit from the same model.
                //Either copy how python's diamond inheritance system works, or remove multiple inheritance.
                traveledModels.Step(parentName);
                var parentModel = cache.Request(parentName, traveledModels);
                CopyComponentsFrom(parentModel.Components);
            }
        }
        
        foreach (XmlNode componentNode in entityNode.ChildNodes)
        {
            if (componentNode.NodeType != XmlNodeType.Element) continue;

            foreach (XmlNode fieldNode in componentNode.ChildNodes)
            {
                if (fieldNode.NodeType != XmlNodeType.Element) continue;
                SetField(componentNode.Name, fieldNode.Name, fieldNode.InnerText);
            }
        }
    }
}
using System.Xml;

namespace ECSLib.XML;

internal class EntityModel
{
    private const string ParentAttributeName = "Parent";
    private const string ParentAttributeSeparator = ";;";

    private readonly XmlNode _node;
    public string Name => _node.Name;
    
    /// <summary>
    /// Maps component type name to a dict which maps field name to field value.
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> Components { get; } = [];

    public List<EntityModel> Parents { get; } = [];
    
    /// <summary>
    /// If true, this model has been verified for inheritance loops in its parents and none were found.
    /// </summary>
    public bool ClearedOfLoops { get; set; }

    /// <summary>
    /// Constructs the entity node initializing <see cref="Name"/> and <see cref="Parents"/>, without resolving fields.
    /// </summary>
    public EntityModel(XmlNode entityNode, ModelCache cache)
    {
        _node = entityNode;

        var parentAttribute = entityNode.Attributes?[ParentAttributeName];
        if (parentAttribute != null)
        {
            var parents = parentAttribute.Value.Split(ParentAttributeSeparator);
            Parents.AddRange(parents.Select(cache.Request));
        }
    }
    
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

    /// <summary>
    /// Fills <see cref="Components"/> considering inheritance. Assumes parents' fields are already resolved.
    /// </summary>
    public void ResolveFields()
    {
        foreach (var parent in Parents)
        {
            CopyComponentsFrom(parent.Components);
        }
        
        foreach (XmlNode componentNode in _node.ChildNodes)
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
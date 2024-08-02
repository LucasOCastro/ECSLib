using System.Xml;
using ECSLib.XML.Exceptions;
using ECSLib.XML.ValueEmitters;

namespace ECSLib.XML;

internal class EntityModel
{
    private const string ParentAttributeName = "Parent";
    private const string ParentAttributeSeparator = ";;";
    private const string ListItemNodeName = "li";

    private readonly XmlNode _node;
    public string Name => _node.Name;
    
    /// <summary>
    /// Maps component type name to a dict which maps field name to field value.
    /// </summary>
    public Dictionary<string, Dictionary<string, IValueEmitter>> Components { get; } = [];

    public string[] Parents { get; }
    
    /// <summary>
    /// If true, this model has been verified for inheritance loops in its parents and none were found.
    /// </summary>
    public bool ClearedOfLoops { get; set; }

    /// <summary>
    /// Constructs the entity node initializing <see cref="Name"/> and <see cref="Parents"/>, without resolving fields.
    /// </summary>
    public EntityModel(XmlNode entityNode)
    {
        _node = entityNode;
        Parents = entityNode.Attributes?[ParentAttributeName]?.Value.Split(ParentAttributeSeparator) ?? [];
    }

    private Dictionary<string, IValueEmitter> GetComponent(string componentType)
    {
        if (!Components.TryGetValue(componentType, out var fields))
        {
            fields = new();
            Components.Add(componentType, fields);
        }
        return fields;
    }

    private void SetField(string componentType, string field, IValueEmitter emitter)
    {
        var fields = GetComponent(componentType);
        fields[field] = emitter;
    }

    //TODO currently for lists and such it is adding instead of replacing, add a Inherit=false attribute to field
    private void CopyComponentsFrom(Dictionary<string, Dictionary<string, IValueEmitter>> components)
    {
        foreach (var (type, fromFields) in components)
        foreach (var (field, value) in fromFields)
            SetField(type, field, value);
    }

    private static IValueEmitter GetEmitterForFieldValue(XmlNode fieldNode)
    {
        if (fieldNode is XmlElement {IsEmpty: true})
            return new EmptyValueEmitter();
        
        if (fieldNode.ChildNodes is [{ NodeType: XmlNodeType.Text }])
            return new TextValueEmitter(fieldNode.ChildNodes[0]?.Value ?? "");
        
        bool isList = false;
        List<IValueEmitter> multipleValues = [];
        foreach (XmlNode itemNode in fieldNode.ChildNodes)
        {
            if (itemNode.NodeType != XmlNodeType.Element) continue;

            if (itemNode.Name == ListItemNodeName) isList = true;
            else if (isList) throw new NodeHasListAndNonListChildrenException(fieldNode);
            multipleValues.Add(GetEmitterForFieldValue(itemNode));
        }

        return new CollectionValueEmitter(multipleValues);
    }
    
    /// <summary>
    /// Fills <see cref="Components"/> considering inheritance. Assumes parents' fields are already resolved.
    /// </summary>
    public void ResolveFields(ModelCache cache)
    {
        foreach (var parent in Parents)
        {
            CopyComponentsFrom(cache.Request(parent).Components);
        }
        
        foreach (XmlNode componentNode in _node.ChildNodes)
        {
            if (componentNode.NodeType != XmlNodeType.Element) continue;

            foreach (XmlNode fieldNode in componentNode.ChildNodes)
            {
                if (fieldNode.NodeType != XmlNodeType.Element) continue;
                var emitter = GetEmitterForFieldValue(fieldNode);
                SetField(componentNode.Name, fieldNode.Name, emitter);
            }
        }
    }
}
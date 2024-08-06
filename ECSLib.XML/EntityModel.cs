using System.Xml;
using ECSLib.XML.ValueEmitters;

namespace ECSLib.XML;

internal class EntityModel
{
    private const string ParentAttributeName = "Parent";
    private const string ParentAttributeSeparator = ";;";
    private const string InheritAttributeName = "Inherit";
    private const string IgnoreAttributeName = "Ignore";

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

    private void CopyComponentsFrom(Dictionary<string, Dictionary<string, IValueEmitter>> components)
    {
        foreach (var (type, fromFields) in components)
        {
            var comp = GetComponent(type);
            foreach (var (field, value) in fromFields)
                comp[field] = value.Copy();
        }
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

            if (bool.TryParse(componentNode.Attributes?[IgnoreAttributeName]?.Value, out var compIgnore)
                && compIgnore)
            {
                Components.Remove(componentNode.Name);
                continue;
            }
            
            if (bool.TryParse(componentNode.Attributes?[InheritAttributeName]?.Value, out var compInherit) 
                && !compInherit)
                Components.Remove(componentNode.Name);

            var comp = GetComponent(componentNode.Name);
            foreach (XmlNode fieldNode in componentNode.ChildNodes)
            {
                if (fieldNode.NodeType != XmlNodeType.Element) continue;
                var emitter = ValueEmitterUtility.GetEmitterForNode(fieldNode);
                
                //If the field is already filled, is mergeable and the inherit attribute is true,
                //merge the two new value with the current value instead of replacing
                if (GetComponent(componentNode.Name).TryGetValue(fieldNode.Name, out var currentEmitter)
                    && currentEmitter is IMergeableValueEmitter mergeable
                    && bool.TryParse(fieldNode.Attributes?[InheritAttributeName]?.Value, out var fieldInherit)
                    && fieldInherit)
                {
                    mergeable.MergeWith(emitter);
                    continue;
                }

                comp[fieldNode.Name] = emitter;
            }
        }
    }
}
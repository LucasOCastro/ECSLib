using System.Xml;
using ECSLib.XML.Exceptions;

namespace ECSLib.XML;

/// <summary>
/// Stores xml nodes for entity factory definitions.
/// </summary>
internal class FactoryXmlRegistry
{
    private const string EntityCollectionNodeName = "Defs";
    
    private readonly Dictionary<string, XmlNode> _definitions = new();

    public XmlNode Get(string name) => _definitions[name];

    /// <summary>
    /// Registers a factory definition node uniquely identified by its name.
    /// </summary>
    /// <exception cref="DuplicatedEntityDocumentNameException">Thrown if two definitions have the same name.</exception>
    private void Register(XmlNode factoryDefinitionNode)
    {
        var name = factoryDefinitionNode.Name;
        if (!_definitions.TryAdd(name, factoryDefinitionNode))
        {
            throw new DuplicatedEntityDocumentNameException(name);
        }
    }

    /// <summary>
    /// Registers all factory definitions contained in the document provided.
    /// Invalid documents are ignored.
    /// </summary>
    /// <exception cref="DuplicatedEntityDocumentNameException">Thrown if two definitions have the same name.</exception>
    public void RegisterDocument(XmlDocument document)
    {
        var root = document.DocumentElement;
        if (root == null) return;

        if (root.Name == EntityCollectionNodeName)
        {
            foreach (XmlNode child in root.ChildNodes)
            {
                if (child.NodeType != XmlNodeType.Element) continue;
                Register(child);
            }
        }
        else Register(root);
    }

    /// <summary>
    /// Registers all factory definitions in each of the documents provided.
    /// </summary>
    /// <exception cref="DuplicatedEntityDocumentNameException">Thrown if two definitions have the same name.</exception>
    public void RegisterDocuments(IEnumerable<XmlDocument> documents)
    {
        foreach (var doc in documents)
        {
            RegisterDocument(doc);
        }
    }

    public IEnumerable<string> AllNames => _definitions.Keys;
    public IEnumerable<XmlNode> AllDefinitions => _definitions.Values;

    public void Clear() => _definitions.Clear();
}
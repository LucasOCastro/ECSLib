using System.Xml;
using ECSLib.XML.Exceptions;

namespace ECSLib.XML;

/// <summary>
/// Stores <see cref="XmlDocument"/>s uniquely identified by the root node name.
/// </summary>
internal class XmlRegistry
{
    private readonly Dictionary<string, XmlDocument> _documents = new();

    public XmlDocument Get(string name) => _documents[name];

    public void Register(XmlDocument document)
    {
        var name = document.DocumentElement.Name;
        if (!_documents.TryAdd(name, document))
        {
            throw new DuplicatedEntityDocumentNameException(name);
        }
    }

    public void Register(IEnumerable<XmlDocument> documents)
    {
        foreach (var doc in documents)
        {
            Register(doc);
        }
    }

    public IEnumerable<string> AllDocNames => _documents.Keys;
    public IEnumerable<XmlDocument> AllDocs => _documents.Values;
    public IEnumerable<KeyValuePair<string, XmlDocument>> AllPairs => _documents;

    public void Clear() => _documents.Clear();
}
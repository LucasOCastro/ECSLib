using System.Collections;

namespace ECSLib.XML;

/// <summary>
/// Stores models for factory generation.
/// </summary>
internal class ModelCache : IEnumerable<EntityModel>
{
    private readonly Dictionary<string, EntityModel> _models = new();

    private readonly XmlRegistry _xmlStorage;
    
    public ModelCache(XmlRegistry xmlStorage)
    {
        _xmlStorage = xmlStorage;
    }

    //TODO detect loops
    /// <summary>
    /// Generate a model if it is not already registered, then return it.
    /// </summary>
    public EntityModel Request(string name)
    {
        if (_models.TryGetValue(name, out var model)) return model;

        var doc = _xmlStorage.Get(name);
        model = new(doc.DocumentElement, this);
        _models.Add(name, model);
        return model;
    }

    /// <summary>
    /// Loads a model for each document in <see cref="_xmlStorage"/>.
    /// </summary>
    public void LoadAll()
    {
        foreach (var name in _xmlStorage.AllDocNames)
        {
            Request(name);
        }
    }

    public IEnumerator<EntityModel> GetEnumerator() => _models.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Clear() => _models.Clear();
}
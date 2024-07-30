using System.Collections;

namespace ECSLib.XML;

/// <summary>
/// Stores models for factory generation.
/// </summary>
internal class ModelCache : IEnumerable<EntityModel>
{
    private readonly Dictionary<string, EntityModel> _models = new();

    private readonly FactoryXmlRegistry _xmlStorage;
    
    public ModelCache(FactoryXmlRegistry xmlStorage)
    {
        _xmlStorage = xmlStorage;
    }
    
    /// <summary>
    /// Generate a model if it is not already registered, then return it.
    /// </summary>
    public EntityModel Request(string name, TravelLog traveledModels)
    {
        if (_models.TryGetValue(name, out var model)) return model;
        
        var def = _xmlStorage.Get(name);
        model = new(def, this, traveledModels);
        _models.Add(name, model);
        return model;
    }

    /// <summary>
    /// Loads a model for each document in <see cref="_xmlStorage"/>.
    /// </summary>
    public void LoadAll()
    {
        foreach (var name in _xmlStorage.AllNames)
        {
            Request(name, new(name));
        }
    }

    public IEnumerator<EntityModel> GetEnumerator() => _models.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Clear() => _models.Clear();
}
using ECSLib.Extensions;

namespace ECSLib.Archetypes;

internal class ArchetypeStorage
{
    private readonly List<Archetype> _archetypes = new();
    private readonly Dictionary<Archetype, int> _archetypeToIndex = new();
    private readonly Dictionary<Type, HashSet<int>> _componentTypeToArchetypeIndices = new();

    /// <returns>Every component type registered in the archetype with the queried id.</returns>
    public IReadOnlySet<Type> GetAllComponentsInArchetype(int id)
    {
        return _archetypes[id].Components;
    }

    /// <summary>
    /// If an archetype is already registered for this collection of types, retrieve its id.
    /// Otherwise, register a new archetype and return the newly registered id.
    /// </summary>
    /// <returns>The id to the unique archetype for the types collection.</returns>
    public int GetOrCreateArchetype(IEnumerable<Type> types)
    {
        Archetype targetArchetype = new(types);
        if (_archetypeToIndex.TryGetValue(targetArchetype, out int index))
        {
            return index;
        }

        index = _archetypes.Count;
        _archetypeToIndex.Add(targetArchetype, index);
        foreach (var type in targetArchetype.Components)
        {
            var set = _componentTypeToArchetypeIndices.GetOrAddNew(type);
            set.Add(index);
        }
        _archetypes.Add(targetArchetype);
        return index;
    }
    
    #region GETTERS

    /// <returns>
    /// All archetype ids which contain the componentType.
    /// </returns>
    public IEnumerable<int> GetArchetypesWith(Type componentType)
    {
        if (!_componentTypeToArchetypeIndices.TryGetValue(componentType, out var indices))
        {
            yield break;
        }

        foreach (int index in indices)
        {
            yield return index;
        }
    }
    
    /// <returns>
    /// All archetype ids which contain all of the componentTypes, and more.
    /// </returns>
    public IEnumerable<int> GetArchetypesWithAll(IEnumerable<Type> componentTypes)
    {
        var indices = componentTypes.Select(t => _componentTypeToArchetypeIndices[t]).Intersection();
        foreach (int index in indices)
        {
            yield return index;
        }
    }
    
    /// <returns>
    /// All archetype ids which contain at least one type from componentTypes.
    /// </returns>
    public IEnumerable<int> GetArchetypesWithAny(IEnumerable<Type> componentTypes)
    {
        var indices = componentTypes.Select(t => _componentTypeToArchetypeIndices[t]).Union();
        foreach (int index in indices)
        {
            yield return index;
        }
    }

    #endregion
}
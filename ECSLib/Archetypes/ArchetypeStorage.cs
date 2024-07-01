using ECSLib.Extensions;

namespace ECSLib.Archetypes;

internal class ArchetypeStorage
{
    private readonly List<Archetype> _archetypes = [];
    private readonly Dictionary<Archetype, int> _archetypeToIndex = new();
    private readonly Dictionary<Type, HashSet<int>> _componentTypeToArchetypeIndices = new();

    /// <returns>Every component type registered in the archetype with the queried id.</returns>
    /// <exception cref="IndexOutOfRangeException">Thrown if <see cref="id"/> is not a valid registered id.</exception>
    public IReadOnlySet<Type> GetAllComponentsInArchetype(int id) =>
        id >= 0 && id < _archetypes.Count
            ? _archetypes[id].Components
            : throw new IndexOutOfRangeException($"{nameof(id)} of value {id} is not registered in {nameof(ArchetypeStorage)}");

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
    
    /// <returns>
    /// A new <see cref="HashSet{T}"/> with all archetype ids which contain the componentType.
    /// </returns>
    public HashSet<int> GetArchetypesWith(Type componentType) =>
        _componentTypeToArchetypeIndices.TryGetValue(componentType, out var indices)
            ? indices
            : new();

    /// <returns>
    /// A new <see cref="HashSet{T}"/> with all archetype ids which contains all the componentTypes, and more.
    /// </returns>
    public HashSet<int> GetArchetypesWithAll(IEnumerable<Type> componentTypes) =>
        componentTypes.Select(GetArchetypesWith).Intersection();

    /// <returns>
    /// A new <see cref="HashSet{T}"/> with all archetype ids which contains at least one type from componentTypes.
    /// </returns>
    public HashSet<int> GetArchetypesWithAny(IEnumerable<Type> componentTypes) =>
        componentTypes.Select(GetArchetypesWith).Union();
}
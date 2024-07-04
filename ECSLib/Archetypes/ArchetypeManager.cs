using ECSLib.Components.Exceptions;
using ECSLib.Entities;
using ECSLib.Extensions;

namespace ECSLib.Archetypes;

internal class ArchetypeManager
{
    #region COLLECTIONS 
    /// <summary> Stores the archetypes themselves. The empty archetype begins initialized in index 0. </summary>
    private readonly List<Archetype> _archetypes = [new(new([]))];
    
    /// <summary>
    /// Maps the index of an archetype to the set of entities which have this archetype.
    /// The component-less archetype at index 0 is initialized to an empty array.
    /// </summary>
    private readonly List<HashSet<Entity>> _archetypeIndexToEntities = [[]];
    
    /// <summary> Maps each collection of component types to the index of its corresponding archetype. </summary>
    private readonly Dictionary<ArchetypeDefinition, int> _archetypeDefinitionToIndex = [];
    
    /// <summary> Maps each component type to all archetypes which include it. </summary>
    private readonly Dictionary<Type, HashSet<int>> _componentTypeToArchetypeIndices = [];
    
    /// <summary> Maps each entity to its archetype record. </summary>
    private readonly Dictionary<Entity, ArchetypeRecord> _entityToArchetypeRecord = [];
    #endregion
    
    #region ARCHETYPE_OPERATIONS
    
    /// <inheritdoc cref="Components.ComponentCollectionSet.RegisterNew"/>
    private int RegisterNewEntityForArchetype(int archetypeIndex) =>
        _archetypes[archetypeIndex].Components.RegisterNew();
    
    /// <inheritdoc cref="Components.ComponentCollectionSet.FreePosition"/>
    private void FreePositionInArchetype(ArchetypeRecord oldRecord) =>
        _archetypes[oldRecord.ArchetypeIndex].Components.FreePosition(oldRecord.EntityIndexInArchetype);

    /// <summary>
    /// If an archetype is already registered for this collection of types, retrieve its id.
    /// Otherwise, register a new archetype and return the newly registered id.
    /// </summary>
    /// <returns>The id to the unique archetype for the <see cref="componentTypes"/> collection.</returns>
    private int GetOrCreateArchetype(IReadOnlyCollection<Type> componentTypes)
    {
        ArchetypeDefinition targetDef = new(componentTypes);
        if (_archetypeDefinitionToIndex.TryGetValue(targetDef, out int index))
        {
            return index;
        }

        index = _archetypes.Count;
        _archetypeDefinitionToIndex.Add(targetDef, index);
        _archetypeIndexToEntities.Add([]);
        foreach (var type in targetDef.Components)
        {
            var set = _componentTypeToArchetypeIndices.GetOrAddNew(type);
            set.Add(index);
        }
        _archetypes.Add(new(targetDef));
        return index;
    }
    
    /// <summary>
    /// Registers an entity to the archetype storage with the default empty archetype.
    /// </summary>
    public void RegisterEmptyEntity(Entity entity)
    {
        int indexInComponentSet = RegisterNewEntityForArchetype(0);
        _entityToArchetypeRecord.Add(entity, new(0, indexInComponentSet));
        _archetypeIndexToEntities[0].Add(entity);
    }
    
    /// <summary>
    /// Changes the archetype of an entity, copying its components
    /// and managing its storage in the old and the new archetypes.
    /// </summary>
    private void MoveEntityTo(Entity entity, int newArchetypeIndex)
    {
        //Copy components from old archetype to the new archetype
        var oldRecord = _entityToArchetypeRecord[entity];
        
        //Allocate space in the new archetype
        int newIndexInComponentSet = RegisterNewEntityForArchetype(newArchetypeIndex);
        
        //Copy components to the new archetype
        _archetypes[oldRecord.ArchetypeIndex].Components
            .CopyTo(oldRecord.EntityIndexInArchetype, _archetypes[newArchetypeIndex].Components, newIndexInComponentSet);
        
        //Free space in the old archetype
        FreePositionInArchetype(oldRecord);
        
        //Remove from the old archetype
        _archetypeIndexToEntities[oldRecord.ArchetypeIndex].Remove(entity);

        //Add to the new archetype
        _archetypeIndexToEntities[newArchetypeIndex].Add(entity);
        _entityToArchetypeRecord[entity] = new(newArchetypeIndex, newIndexInComponentSet);
    }
    
    /// <summary>
    /// Unregisters an entity from the archetype storage.
    /// </summary>
    public void Unregister(Entity entity)
    {
        var currentArchetype = _entityToArchetypeRecord[entity].ArchetypeIndex;
        var oldRecord = _entityToArchetypeRecord[entity]; 
        _entityToArchetypeRecord.Remove(entity);
        _archetypeIndexToEntities[currentArchetype].Remove(entity);
        FreePositionInArchetype(oldRecord);
    }
    
    #endregion
    
    #region ENTITY_COMPONENTS
    
    /// <returns>The type of every component an entity has in its archetype.</returns>
    public IReadOnlySet<Type> GetAllComponentTypes(Entity entity)
    {
        var archetype = _entityToArchetypeRecord[entity].ArchetypeIndex;
        return _archetypes[archetype].Definition.Components;
    }
    
    /// <summary>
    /// Updates an entity's archetype to another which includes all previous component types plus the new component.
    /// Stores a new component either by default value or with the value provided.
    /// </summary>
    /// <exception cref="DuplicatedComponentException">Thrown if tried to add a component the entity already has.</exception>
    private void AddComponent(Entity entity, Type componentType)
    {
        var oldComponents = GetAllComponentTypes(entity);
        var newComponents = oldComponents.Include(componentType);
        if (oldComponents.SetEquals(newComponents))
        {
            throw new DuplicatedComponentException(componentType, entity);
        }

        var newArchetype = GetOrCreateArchetype(newComponents);
        MoveEntityTo(entity, newArchetype);
    }
    
    /// <inheritdoc cref="AddComponent"/>
    public void AddComponent<TComponent>(Entity entity, TComponent component) where TComponent : struct
    {
        AddComponent(entity, typeof(TComponent));
        GetComponent<TComponent>(entity) = component;
    }

    /// <summary>
    /// Updates an entity's archetype to another which includes all previous component types except the one removed.
    /// Frees the space in storage corresponding to that component.
    /// </summary>
    /// <exception cref="MissingComponentException">Thrown if tried to remove a component not registered to the entity.</exception>
    public void RemoveComponent(Entity entity, Type componentType)
    {
        var oldComponents = GetAllComponentTypes(entity);
        var newComponents = oldComponents.Except(componentType);
        if (oldComponents.SetEquals(newComponents))
        {
            throw new MissingComponentException(componentType, entity);
        }
        
        var newArchetype = GetOrCreateArchetype(newComponents);
        MoveEntityTo(entity, newArchetype);
    }

    /// <inheritdoc cref="RemoveComponent"/>
    public void RemoveComponent<TComponent>(Entity entity) where TComponent : struct =>
        RemoveComponent(entity, typeof(TComponent));
    
    /// <returns><inheritdoc cref="Components.ComponentCollectionSet.Get{T}" path="/returns"/></returns>
    /// <exception cref="MissingComponentException">Thrown if tried to get a component the entity does not have.</exception>
    public ref TComponent GetComponent<TComponent>(Entity entity) where TComponent : struct
    {
        var record = _entityToArchetypeRecord[entity];
        if (!_archetypes[record.ArchetypeIndex].Definition.Components.Contains(typeof(TComponent)))
        {
            throw new MissingComponentException(typeof(TComponent), entity);
        }
        return ref _archetypes[record.ArchetypeIndex].Components.Get<TComponent>(record.EntityIndexInArchetype);
    }
    #endregion
    
    #region QUERYING
    
    /// <returns>
    /// A new <see cref="HashSet{T}"/> with all archetype ids which contain the componentType.
    /// </returns>
    public HashSet<int> GetArchetypesWith(Type componentType) =>
        _componentTypeToArchetypeIndices.TryGetValue(componentType, out var indices)
            ? indices
            : [];

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
    
    /// <returns>A new <see cref="HashSet{T}"/> with all archetype ids which match the query.</returns>
    private HashSet<int> QueryArchetypes(Query query)
    {
        HashSet<int> result;
        if (query.HasAll)
        {
            result = GetArchetypesWithAll(query.GetAll());
            if (query.HasAny)
                result.IntersectWith(GetArchetypesWithAny(query.GetAny()));   
        }
        else if (query.HasAny)
        {
            result = GetArchetypesWithAny(query.GetAny());
        }
        else return [];
        
        result.ExceptWith(GetArchetypesWithAny(query.GetNone()));
        return result;
    }

    /// <returns>All entities whose archetypes match the query.</returns>
    public IEnumerable<Entity> QueryEntities(Query query) =>
        QueryArchetypes(query).SelectMany(a => _archetypeIndexToEntities[a]);
    
    #endregion
}
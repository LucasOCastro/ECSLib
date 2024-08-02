using ECSLib.Components;
using ECSLib.Components.Exceptions;
using ECSLib.Entities;
using ECSLib.Extensions;

namespace ECSLib.Archetypes;

// ReSharper disable once PartialTypeWithSinglePart
internal partial class ArchetypeManager
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
    
    
    /// <summary> Maps each entity to its archetype record, and each archetype record to its entity. </summary>
    private readonly BidirectionalMap<Entity, ArchetypeRecord> _entitiesRecords = new();
    
    #endregion
    
    #region ARCHETYPE_OPERATIONS
    
    /// <inheritdoc cref="Components.ComponentCollectionSet.RegisterNew"/>
    private int RegisterNewEntityForArchetype(int archetypeIndex) =>
        _archetypes[archetypeIndex].Components.RegisterNew();
    
    /// <summary><inheritdoc cref="Components.ComponentCollectionSet.FreePosition" path="/summary"/></summary>
    private void FreePositionInArchetype(ArchetypeRecord oldRecord)
    {
        int movedIndex = _archetypes[oldRecord.ArchetypeIndex].Components.FreePosition(oldRecord.EntityIndexInArchetype);
        if (movedIndex >= 0)
        {
            var movedRecord = oldRecord with { EntityIndexInArchetype = movedIndex };
            var movedEntity = _entitiesRecords[movedRecord];
            _entitiesRecords.Set(movedEntity, oldRecord);
        }
    }

    /// <summary>
    /// If an archetype is already registered for this collection of types, retrieve its id.
    /// Otherwise, register a new archetype and return the newly registered id.
    /// </summary>
    /// <returns>The id to the unique archetype for the <see cref="componentTypes"/> collection.</returns>
    private int GetOrCreateArchetype(IEnumerable<Type> componentTypes)
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
    /// Gets an archetype from an added or removed component type coming from an existing archetype,
    /// following the archetype graph or registering a new edge.
    /// </summary>
    /// <param name="archetype">The original archetype.</param>
    /// <param name="componentType">The new component which was added or removed form the original archetype.</param>
    /// <param name="add">Whether the component was added or removed from the original archetype.</param>
    /// <returns></returns>
    private int GetOrCreateArchetypeFrom(int archetype, Type componentType, bool add)
    {
        ArchetypeEdge edge = add ? new(componentType, null) : new(null, componentType);

        if (_archetypes[archetype].Edges.TryGetValue(edge, out var newArchetype))
        {
            return newArchetype;
        }
        
        var oldComponents = _archetypes[archetype].Definition.Components;
        var newComponents = add ? oldComponents.Append(componentType) : oldComponents.Except(componentType);
        newArchetype = GetOrCreateArchetype(newComponents);
        _archetypes[archetype].Edges.Add(edge, newArchetype);
        return newArchetype;
    }
    
    /// <summary>
    /// Registers an entity to the archetype storage with the default empty archetype.
    /// </summary>
    public void RegisterEmptyEntity(Entity entity)
    {
        int indexInComponentSet = RegisterNewEntityForArchetype(0);
        ArchetypeRecord record = new(0, indexInComponentSet);
        _entitiesRecords.Add(entity, record);
        _archetypeIndexToEntities[0].Add(entity);
    }
    
    /// <summary>
    /// Changes the archetype of an entity, copying its components
    /// and managing its storage in the old and the new archetypes.
    /// </summary>
    private void MoveEntityTo(Entity entity, int newArchetypeIndex)
    {
        //Allocate space in the new archetype
        int newIndexInComponentSet = RegisterNewEntityForArchetype(newArchetypeIndex);
        
        //Copy components to the new archetype
        var oldRecord = _entitiesRecords[entity];// _entityToArchetypeRecord[entity];
        _archetypes[oldRecord.ArchetypeIndex].Components
            .CopyTo(oldRecord.EntityIndexInArchetype, _archetypes[newArchetypeIndex].Components, newIndexInComponentSet);
        
        //Free space in the old archetype
        FreePositionInArchetype(oldRecord);
        
        //Remove from the old archetype
        _archetypeIndexToEntities[oldRecord.ArchetypeIndex].Remove(entity);
        
        //Add to the new archetype
        _archetypeIndexToEntities[newArchetypeIndex].Add(entity);
        ArchetypeRecord newRecord = new(newArchetypeIndex, newIndexInComponentSet);
        _entitiesRecords.Set(entity, newRecord);
    }
    
    /// <summary>
    /// Unregisters an entity from the archetype storage.
    /// </summary>
    public void Unregister(Entity entity)
    {
        var oldRecord = _entitiesRecords[entity];
        _archetypeIndexToEntities[oldRecord.ArchetypeIndex].Remove(entity);
        _entitiesRecords.Remove(entity);
        FreePositionInArchetype(oldRecord);
    }
    
    #endregion
    
    #region ENTITY_COMPONENTS
    
    /// <returns>The type of every component an entity has in its archetype.</returns>
    public IReadOnlySet<Type> GetAllComponentTypes(Entity entity)
    {
        var archetype = _entitiesRecords[entity].ArchetypeIndex;
        return _archetypes[archetype].Definition.Components;
    }
    
    /// <summary>
    /// Updates an entity's archetype to another which includes all previous component types plus the new component.
    /// Stores a new component either by default value or with the value provided.
    /// </summary>
    /// <exception cref="DuplicatedComponentException">Thrown if tried to add a component the entity already has.</exception>
    private void AddComponent(Entity entity, Type componentType)
    {
        var oldArchetype = _entitiesRecords[entity].ArchetypeIndex;
        if (_archetypes[oldArchetype].Definition.Components.Contains(componentType))
        {
            throw new DuplicatedComponentException(componentType, entity);
        }

        var newArchetype = GetOrCreateArchetypeFrom(oldArchetype, componentType, add: true);
        MoveEntityTo(entity, newArchetype);
    }
    
    /// <inheritdoc cref="AddComponent"/>
    public void AddComponent<TComponent>(Entity entity, TComponent component) where TComponent : struct
    {
        AddComponent(entity, typeof(TComponent));
        //I NEED to do this, because otherwise the bytes aren't filled and everything is 0.
        GetComponent<TComponent>(entity) = component;
    }
    
    /// <inheritdoc cref="AddComponent"/>
    public void AddComponent<TComponent>(Entity entity) where TComponent : struct =>
        AddComponent(entity, new TComponent());

    /// <summary>
    /// Updates an entity's archetype to another which includes all previous component types except the one removed.
    /// Frees the space in storage corresponding to that component.
    /// </summary>
    /// <exception cref="MissingComponentException">Thrown if tried to remove a component not registered to the entity.</exception>
    public void RemoveComponent(Entity entity, Type componentType)
    {
        var oldArchetype = _entitiesRecords[entity].ArchetypeIndex;
        if (!_archetypes[oldArchetype].Definition.Components.Contains(componentType))
        {
            throw new MissingComponentException(componentType, entity);
        }
        
        var newArchetype = GetOrCreateArchetypeFrom(oldArchetype, componentType, add: false);
        MoveEntityTo(entity, newArchetype);
    }

    /// <inheritdoc cref="RemoveComponent"/>
    public void RemoveComponent<TComponent>(Entity entity) where TComponent : struct =>
        RemoveComponent(entity, typeof(TComponent));
    
    /// <returns><inheritdoc cref="Components.ComponentCollectionSet.Get{T}" path="/returns"/></returns>
    /// <exception cref="MissingComponentException">Thrown if tried to get a component the entity does not have.</exception>
    public ref TComponent GetComponent<TComponent>(Entity entity) where TComponent : struct
    {
        var record = _entitiesRecords[entity];
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
    private HashSet<int> GetArchetypesWith(Type componentType) =>
        _componentTypeToArchetypeIndices.TryGetValue(componentType, out var indices)
            ? indices
            : [];

    private static Comp<T> GetRef<T>(Span<T> span, int i) where T : struct => 
        span.IsEmpty ? new() : new(ref span[i]); 
    
    private readonly HashSet<int> _anySet = [];
    /// <returns>A new <see cref="HashSet{T}"/> with all archetype ids which match the query.</returns>
    public void QueryArchetypes(Query query, HashSet<int> result)
    {
        if (query.All.Length == 0) return;
        
        result.UnionWith(GetArchetypesWith(query.All[0]));
        if (result.Count == 0) return;
        for (int i = 1; i < query.All.Length; i++)
        {
            result.IntersectWith(GetArchetypesWith(query.All[i]));
            if (result.Count == 0) return;
        }

        if (query.Any.Length > 0)
        {
            foreach (var type in query.Any)
            {
                _anySet.UnionWith(GetArchetypesWith(type));
            }
            result.IntersectWith(_anySet);
            _anySet.Clear();
            if (result.Count == 0) return;
        }
        
        foreach (var type in query.None)
        {
            result.ExceptWith(GetArchetypesWith(type));
            if (result.Count == 0) return;
        }
    }

    private readonly HashSet<int> _queryResultSet = [];
    
    public void Query(Query query, QueryAction action) 
    {
        QueryArchetypes(query, _queryResultSet);
        foreach (var archetype in _queryResultSet)
        {
            var components = _archetypes[archetype].Components;
            for (int i = 0; i < components.Count; i++)
            {
                var entity = _entitiesRecords[new ArchetypeRecord(archetype, i)];
                action(entity);
            }
        }
        _queryResultSet.Clear();
    }
    
    public void Query<T1>(Query query, QueryAction<T1> action) 
    where T1 : struct
{
    QueryArchetypes(query, _queryResultSet);
    foreach (var archetype in _queryResultSet)
    {
        var components = _archetypes[archetype].Components;
        var span1 = components.GetFullSpan<T1>();
        for (int i = 0; i < components.Count; i++)
        {
            var entity = _entitiesRecords[new ArchetypeRecord(archetype, i)];
            var ref1 = GetRef(span1, i);
            action(entity, ref ref1);
        }
    }
    _queryResultSet.Clear();
}

public void Query<T1, T2>(Query query, QueryAction<T1, T2> action) 
    where T1 : struct
    where T2 : struct
{
    QueryArchetypes(query, _queryResultSet);
    foreach (var archetype in _queryResultSet)
    {
        var components = _archetypes[archetype].Components;
        var span1 = components.GetFullSpan<T1>();
        var span2 = components.GetFullSpan<T2>();
        for (int i = 0; i < components.Count; i++)
        {
            var entity = _entitiesRecords[new ArchetypeRecord(archetype, i)];
            var ref1 = GetRef(span1, i);
            var ref2 = GetRef(span2, i);
            action(entity, ref ref1, ref ref2);
        }
    }
    _queryResultSet.Clear();
}

public void Query<T1, T2, T3>(Query query, QueryAction<T1, T2, T3> action) 
    where T1 : struct
    where T2 : struct
    where T3 : struct
{
    QueryArchetypes(query, _queryResultSet);
    foreach (var archetype in _queryResultSet)
    {
        var components = _archetypes[archetype].Components;
        var span1 = components.GetFullSpan<T1>();
        var span2 = components.GetFullSpan<T2>();
        var span3 = components.GetFullSpan<T3>();
        for (int i = 0; i < components.Count; i++)
        {
            var entity = _entitiesRecords[new ArchetypeRecord(archetype, i)];
            var ref1 = GetRef(span1, i);
            var ref2 = GetRef(span2, i);
            var ref3 = GetRef(span3, i);
            action(entity, ref ref1, ref ref2, ref ref3);
        }
    }
    _queryResultSet.Clear();
}

public void Query<T1, T2, T3, T4>(Query query, QueryAction<T1, T2, T3, T4> action) 
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
{
    QueryArchetypes(query, _queryResultSet);
    foreach (var archetype in _queryResultSet)
    {
        var components = _archetypes[archetype].Components;
        var span1 = components.GetFullSpan<T1>();
        var span2 = components.GetFullSpan<T2>();
        var span3 = components.GetFullSpan<T3>();
        var span4 = components.GetFullSpan<T4>();
        for (int i = 0; i < components.Count; i++)
        {
            var entity = _entitiesRecords[new ArchetypeRecord(archetype, i)];
            var ref1 = GetRef(span1, i);
            var ref2 = GetRef(span2, i);
            var ref3 = GetRef(span3, i);
            var ref4 = GetRef(span4, i);
            action(entity, ref ref1, ref ref2, ref ref3, ref ref4);
        }
    }
    _queryResultSet.Clear();
}

public void Query<T1, T2, T3, T4, T5>(Query query, QueryAction<T1, T2, T3, T4, T5> action) 
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
    where T5 : struct
{
    QueryArchetypes(query, _queryResultSet);
    foreach (var archetype in _queryResultSet)
    {
        var components = _archetypes[archetype].Components;
        var span1 = components.GetFullSpan<T1>();
        var span2 = components.GetFullSpan<T2>();
        var span3 = components.GetFullSpan<T3>();
        var span4 = components.GetFullSpan<T4>();
        var span5 = components.GetFullSpan<T5>();
        for (int i = 0; i < components.Count; i++)
        {
            var entity = _entitiesRecords[new ArchetypeRecord(archetype, i)];
            var ref1 = GetRef(span1, i);
            var ref2 = GetRef(span2, i);
            var ref3 = GetRef(span3, i);
            var ref4 = GetRef(span4, i);
            var ref5 = GetRef(span5, i);
            action(entity, ref ref1, ref ref2, ref ref3, ref ref4, ref ref5);
        }
    }
    _queryResultSet.Clear();
}

public void Query<T1, T2, T3, T4, T5, T6>(Query query, QueryAction<T1, T2, T3, T4, T5, T6> action) 
    where T1 : struct
    where T2 : struct
    where T3 : struct
    where T4 : struct
    where T5 : struct
    where T6 : struct
{
    QueryArchetypes(query, _queryResultSet);
    foreach (var archetype in _queryResultSet)
    {
        var components = _archetypes[archetype].Components;
        var span1 = components.GetFullSpan<T1>();
        var span2 = components.GetFullSpan<T2>();
        var span3 = components.GetFullSpan<T3>();
        var span4 = components.GetFullSpan<T4>();
        var span5 = components.GetFullSpan<T5>();
        var span6 = components.GetFullSpan<T6>();
        for (int i = 0; i < components.Count; i++)
        {
            var entity = _entitiesRecords[new ArchetypeRecord(archetype, i)];
            var ref1 = GetRef(span1, i);
            var ref2 = GetRef(span2, i);
            var ref3 = GetRef(span3, i);
            var ref4 = GetRef(span4, i);
            var ref5 = GetRef(span5, i);
            var ref6 = GetRef(span6, i);
            action(entity, ref ref1, ref ref2, ref ref3, ref ref4, ref ref5, ref ref6);
        }
    }
    _queryResultSet.Clear();
}
    
    #endregion
}
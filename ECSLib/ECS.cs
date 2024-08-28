using System.Reflection;
using ECSLib.Archetypes;
using ECSLib.Entities;
using ECSLib.Systems;

namespace ECSLib;

// ReSharper disable once PartialTypeWithSinglePart
public sealed partial class ECS
{
    public delegate void OnEntityDestroyedDelegate(Entity entity);
    public event OnEntityDestroyedDelegate? OnEntityDestroyed;
    
    private readonly EntityManager _entityManager = new();
    private readonly ArchetypeManager _archetypeManager = new();
    private readonly SystemManager _systemManager = new();

    /// <param name="registerSystemsViaReflection">
    /// If true, all concrete classes which inherit <see cref="BaseSystem"/>
    /// and have <see cref="Systems.Attributes.ECSSystemClassAttribute"/> will
    /// automatically be registered to execute on <see cref="ProcessSystems()"/>.
    /// </param>
    /// <param name="pipelineEnumType">
    /// If provided, will register the type as the pipeline enum type.
    /// </param>
    /// <param name="registerPipelineEnumTypeViaReflection">
    /// If true and no type is directly passed via <see cref="pipelineEnumType"/>,
    /// will search for an enum type which has <see cref="Systems.Attributes.ECSSystemClassAttribute"/>
    /// and register it as the pipeline enum.
    /// </param>
    /// <seealso cref="RegisterSystemsFrom"/>
    public ECS(bool registerSystemsViaReflection = false, Type? pipelineEnumType = null, bool registerPipelineEnumTypeViaReflection = false)
    {
        if (pipelineEnumType != null)
            _systemManager.PipelineEnumType = pipelineEnumType;
        else if (registerPipelineEnumTypeViaReflection)
            _systemManager.RegisterPipelineEnumType(Assembly.GetCallingAssembly());
        
        if (registerSystemsViaReflection)
            _systemManager.RegisterAllSystems(Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Destroys all entities, freeing up space and emptying the RefPool. 
    /// </summary>
    public void Clear()
    {
        foreach (var entity in _entityManager.AllEntities.ToList())
        {
            DestroyEntity(entity);
        }
    }
    
    #region ENTITIES

    /// <summary>Registers a new entity with no components.</summary>
    /// <returns> The new entity. </returns>
    public Entity CreateEntity()
    {
        var entity = _entityManager.CreateEntity();
        _archetypeManager.RegisterEmptyEntity(entity);
        return entity;
    }

    /// <summary>Registers a new entity with the defined components.</summary>
    /// <remarks><inheritdoc cref="ArchetypeManager.RegisterEntityInArchetype" path="remarks"/></remarks>
    /// <returns> The new entity. </returns>
    public Entity CreateEntityWithComponents(IEnumerable<Type> components)
    {
        var entity = _entityManager.CreateEntity();
        _archetypeManager.RegisterEntityInArchetype(entity, components);
        return entity;
    }
    
    /// <summary> Unregisters an entity and all of its components. </summary>
    public void DestroyEntity(Entity entity)
    {
        _archetypeManager.Unregister(entity);
        _entityManager.RemoveEntity(entity);
        OnEntityDestroyed?.Invoke(entity);
    }

    /// <returns>true if the stored entity is valid, false if it has been destroyed.</returns>
    public bool IsEntityValid(Entity entity) => _entityManager.IsValid(entity);
    
    #endregion
    
    #region COMPONENTS
    
    /// <inheritdoc cref="ArchetypeManager.GetComponent{TComponent}"/>
    public ref TComponent GetComponent<TComponent>(Entity entity) where TComponent : struct
    {
        return ref _archetypeManager.GetComponent<TComponent>(entity);
    }
    
    /// <inheritdoc cref="ArchetypeManager.AddComponent{TComponent}(Entity, TComponent)"/>
    public void AddComponent<TComponent>(Entity entity, TComponent component) where TComponent : struct
    {
        _archetypeManager.AddComponent(entity, component);
    }
    
    /// <inheritdoc cref="ArchetypeManager.AddComponent{TComponent}(Entity, TComponent)"/>
    public void AddComponent<TComponent>(Entity entity) where TComponent : struct
    {
        AddComponent(entity, new TComponent());
    }
    
    /// <inheritdoc cref="ArchetypeManager.RemoveComponent{TComponent}"/>
    public void RemoveComponent<TComponent>(Entity entity) where TComponent : struct
    {
        _archetypeManager.RemoveComponent<TComponent>(entity);
    }

    /// <inheritdoc cref="ArchetypeManager.SetComponent{TComponent}"/>
    public void SetComponent<TComponent>(Entity entity, TComponent value) where TComponent : struct
    {
        _archetypeManager.SetComponent(entity, value);
    }
    
    /// <returns>true if the entity has the given component.</returns>
    public bool HasComponent(Entity entity, Type componentType)
    {
        return _archetypeManager.GetAllComponentTypes(entity).Contains(componentType);
    }

    /// <inheritdoc cref="HasComponent"/>
    public bool HasComponent<TComponent>(Entity entity) where TComponent : struct
    {
        return HasComponent(entity, typeof(TComponent));
    }

    public IReadOnlySet<Type> GetAllComponentTypes(Entity entity)
    {
        return _archetypeManager.GetAllComponentTypes(entity);
    }

    #endregion

    #region  SYSTEMS
    
    /// <inheritdoc cref="SystemManager.RegisterSystem"/>
    public void RegisterSystem(BaseSystem system) => _systemManager.RegisterSystem(system);

    /// /// <summary> Registers a new system to be processed in <see cref="ProcessSystems()"/>.</summary>
    /// <typeparam name="T">The type of the system to be instantiated and registered. Only one system of each type is allowed per world.</typeparam>
    public void RegisterSystem<T>() where T : BaseSystem, new() => _systemManager.RegisterSystem<T>();
    
    /// <inheritdoc cref="SystemManager.RegisterAllSystems"/>
    public void RegisterSystemsFrom(Assembly assembly) => _systemManager.RegisterAllSystems(assembly);
    
    
    /// <inheritdoc cref="SystemManager.Process(ECS)"/>
    public void ProcessSystems() => _systemManager.Process(this);
    
    /// <inheritdoc cref="SystemManager.Process(ECS, int)"/>
    public void ProcessSystems(int pipeline) => _systemManager.Process(this, pipeline);
    
    #endregion
    
    #region QUERYING

    public void Query(Query query, QueryAction action) 
        => _archetypeManager.Query(query, action);
    
    public void Query<T1>(Query query, QueryAction<T1> action)
        where T1 : struct
        => _archetypeManager.Query(query, action);

    public void Query<T1, T2>(Query query, QueryAction<T1, T2> action)
        where T1 : struct
        where T2 : struct
        => _archetypeManager.Query(query, action);

    public void Query<T1, T2, T3>(Query query, QueryAction<T1, T2, T3> action)
        where T1 : struct
        where T2 : struct
        where T3 : struct
        => _archetypeManager.Query(query, action);

    public void Query<T1, T2, T3, T4>(Query query, QueryAction<T1, T2, T3, T4> action)
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        => _archetypeManager.Query(query, action);

    public void Query<T1, T2, T3, T4, T5>(Query query, QueryAction<T1, T2, T3, T4, T5> action)
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        => _archetypeManager.Query(query, action);

    public void Query<T1, T2, T3, T4, T5, T6>(Query query, QueryAction<T1, T2, T3, T4, T5, T6> action)
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
        where T5 : struct
        where T6 : struct
        => _archetypeManager.Query(query, action);
    
    #endregion
    
    #region INTERNAL_COMPONENT_ACCESS

    internal IEnumerable<(Entity entity, IEnumerable<Binary.BinaryComponent> components)> GetAllInfo() =>
        _archetypeManager.GetAllInfo();

    internal void SetData(Entity entity, Type componentType, byte[] componentData) =>
        _archetypeManager.SetData(entity, componentType, componentData);

    #endregion
}
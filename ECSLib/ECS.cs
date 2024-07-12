using System.Reflection;
using ECSLib.Archetypes;
using ECSLib.Entities;
using ECSLib.Systems;

namespace ECSLib;

public sealed partial class ECS
{
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
    public ECS(bool registerSystemsViaReflection = false, Type? pipelineEnumType = null, bool registerPipelineEnumTypeViaReflection = false)
    {
        if (pipelineEnumType != null)
            _systemManager.PipelineEnumType = pipelineEnumType;
        else if (registerPipelineEnumTypeViaReflection)
            _systemManager.RegisterPipelineEnumType(Assembly.GetCallingAssembly());
        
        if (registerSystemsViaReflection)
            _systemManager.RegisterAllSystems(Assembly.GetCallingAssembly());
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
    
    /// <summary> Unregisters an entity and all of its components. </summary>
    public void DestroyEntity(Entity entity)
    {
        _archetypeManager.Unregister(entity);
        _entityManager.RemoveEntity(entity);
    }
    
    #endregion
    
    #region COMPONENTS
    
    /// <inheritdoc cref="ArchetypeManager.GetComponent{TComponent}"/>
    public ref TComponent GetComponent<TComponent>(Entity entity) where TComponent : struct
    {
        return ref _archetypeManager.GetComponent<TComponent>(entity);
    }

    /// <inheritdoc cref="ArchetypeManager.AddComponent{TComponent}"/>
    public void AddComponent<TComponent>(Entity entity, TComponent component = default) where TComponent : struct
    {
        _archetypeManager.AddComponent(entity, component);
    }
    
    /// <inheritdoc cref="ArchetypeManager.RemoveComponent{TComponent}"/>
    public void RemoveComponent<TComponent>(Entity entity) where TComponent : struct
    {
        _archetypeManager.RemoveComponent<TComponent>(entity);
    }

    #endregion

    #region  SYSTEMS
    
    /// <inheritdoc cref="SystemManager.RegisterSystem"/>
    public void RegisterSystem(BaseSystem system) => _systemManager.RegisterSystem(system);

    /// /// <summary> Registers a new system to be processed in <see cref="ProcessSystems()"/>.</summary>
    /// <typeparam name="T">The type of the system to be instantiated and registered. Only one system of each type is allowed per world.</typeparam>
    public void RegisterSystem<T>() where T : BaseSystem, new() => _systemManager.RegisterSystem<T>();
    
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
}
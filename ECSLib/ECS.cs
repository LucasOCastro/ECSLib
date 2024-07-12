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
    /// If true, all concrete classes which inherit <see cref="BaseSystem"/> will automatically
    /// be registered to execute on <see cref="ProcessSystems"/>.
    /// </param>
    public ECS(bool registerSystemsViaReflection = false)
    {
        if (registerSystemsViaReflection)
            ReflectionLoader.RegisterAllSystems(_systemManager, Assembly.GetCallingAssembly());
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

    /// <inheritdoc cref="SystemManager.RegisterSystem{T}"/>
    public void RegisterSystem<T>() where T : BaseSystem, new() => _systemManager.RegisterSystem<T>();
    
    
    /// <inheritdoc cref="SystemManager.Process"/>
    public void ProcessSystems(float dt) => _systemManager.Process(this);
    
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
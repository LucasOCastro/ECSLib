using ECSLib.Archetypes;
using ECSLib.Components;
using ECSLib.Entities;
using ECSLib.Systems;

namespace ECSLib;

public class ECS
{
    private readonly EntityManager _entityManager = new();
    private readonly ComponentManager _componentManager = new();
    private readonly ArchetypeManager _archetypeManager = new();
    private readonly SystemManager _systemManager = new(); 

    /// <returns>
    /// A new entity with no components and returns its identifier.
    /// </returns>
    public Entity CreateEntity()
    {
        var entity = _entityManager.CreateEntity();
        _archetypeManager.Register(entity);
        return entity;
    }

    /// <inheritdoc cref="ComponentManager.GetComponent{TComponent}"/>
    public ref TComponent GetComponent<TComponent>(Entity entity) where TComponent : struct
    {
        return ref _componentManager.GetComponent<TComponent>(entity);
    }

    /// <inheritdoc cref="ComponentManager.AddComponent{TComponent}"/>
    public void AddComponent<TComponent>(Entity entity, TComponent component = default) where TComponent : struct
    {
        _archetypeManager.BeforeComponentAddedTo(entity, typeof(TComponent));
        _componentManager.AddComponent(entity, component);
    }
    
    /// <inheritdoc cref="ComponentManager.RemoveComponent{TComponent}"/>
    public void RemoveComponent<TComponent>(Entity entity) where TComponent : struct
    {
        _archetypeManager.BeforeComponentRemovedFrom(entity, typeof(TComponent));
        _componentManager.RemoveComponent<TComponent>(entity);
    }

    /// <summary>
    /// Unregisters an entity and all of its components.
    /// </summary>
    /// <param name="entity"></param>
    public void DestroyEntity(Entity entity)
    {
        foreach (var componentType in _archetypeManager.GetAllComponentTypes(entity))
        {
            _componentManager.RemoveComponent(entity, componentType);
        }
        _archetypeManager.Unregister(entity);
        _entityManager.RemoveEntity(entity);
    }

    /// <inheritdoc cref="ArchetypeManager.QueryEntities"/>
    public IEnumerable<Entity> Query(Query query) => _archetypeManager.QueryEntities(query);

    /// <inheritdoc cref="SystemManager.RegisterSystem"/>
    public void RegisterSystem(BaseSystem system) => _systemManager.RegisterSystem(system);
    
    /// <inheritdoc cref="SystemManager.Process"/>
    public void ProcessSystems(float dt) => _systemManager.Process(dt, this);
}
using ECSLib.Archetypes;
using ECSLib.Components;
using ECSLib.Entities;

namespace ECSLib;

public class ECS
{
    private readonly EntityManager _entityManager = new();
    private readonly ComponentManager _componentManager = new();
    private readonly ArchetypeManager _archetypeManager = new();

    /// <returns>
    /// A new entity with no components and returns its identifier.
    /// </returns>
    public Entity CreateEntity()
    {
        var entity = _entityManager.CreateEntity();
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
        _archetypeManager.BeforeComponentAddedTo(typeof(TComponent), entity);
        _componentManager.AddComponent(entity, component);
    }
    
    /// <inheritdoc cref="ComponentManager.RemoveComponent{TComponent}"/>
    public void RemoveComponent<TComponent>(Entity entity) where TComponent : struct
    {
        _archetypeManager.BeforeComponentRemovedFrom(typeof(TComponent), entity);
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
}
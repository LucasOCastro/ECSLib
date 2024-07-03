using ECSLib.Entities;
using ECSLib.Extensions;

namespace ECSLib.Components;

public class ComponentManager
{
    private readonly Dictionary<Type, ComponentCollection> _collections = new();

    private ComponentCollection GetCollection(Type type)
    {
        if (!_collections.TryGetValue(type, out var collection))
        {
            collection = new(type);
            _collections.Add(type, collection);
        }
        return collection;
    }
    
    private ComponentCollection GetCollection<TComponent>() => GetCollection(typeof(TComponent));

    /// <inheritdoc cref="ComponentCollection.Get{TComponent}"/>
    public ref TComponent GetComponent<TComponent>(Entity entity) where TComponent : struct =>
        ref GetCollection<TComponent>().Get<TComponent>(entity);

    /// <summary>
    /// Adds a component to an Entity. Uses default constructor by default.
    /// </summary>
    /// <inheritdoc cref="ComponentCollection.Register{TComponent}"/>
    public void AddComponent<TComponent>(Entity entity, TComponent component = default) where TComponent : struct =>
        GetCollection<TComponent>().Register(entity, component);

    /// <summary>
    /// Removes a component from an entity. 
    /// </summary>
    /// <inheritdoc cref="ComponentCollection.Unregister{TComponent}"/>
    public void RemoveComponent<TComponent>(Entity entity) where TComponent : struct =>
        GetCollection<TComponent>().Unregister(entity);

    /// <inheritdoc cref="RemoveComponent{TComponent}"/>
    public void RemoveComponent(Entity entity, Type componentType) => 
        GetCollection(componentType).Unregister(entity);
}
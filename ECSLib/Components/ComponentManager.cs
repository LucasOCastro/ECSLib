using ECSLib.Entities;
using ECSLib.Extensions;

namespace ECSLib.Components;

public class ComponentManager
{
    private readonly Dictionary<Type, ComponentCollection> _collections = new();

    private ComponentCollection GetCollection(Type type) => _collections.GetOrAdd(type, () => new(type));
    
    private ComponentCollection GetCollection<TComponent>() => GetCollection(typeof(TComponent));

    /// <inheritdoc cref="ComponentCollection.Get{TComponent}"/>
    public ref TComponent GetComponent<TComponent>(Entity entity) where TComponent : struct =>
        ref GetCollection<TComponent>().Get<TComponent>(entity);

    /// <summary>
    /// Adds a component to an Entity using its default constructor.
    /// </summary>
    /// <inheritdoc cref="ComponentCollection.Register{TComponent}"/>
    public void AddComponent<TComponent>(Entity entity) where TComponent : struct =>
        GetCollection<TComponent>().Register(entity, new TComponent());
    
    /// <summary>
    /// Adds a component to an Entity.
    /// </summary>
    /// <inheritdoc cref="ComponentCollection.Register{TComponent}"/>
    public void AddComponent<TComponent>(Entity entity, TComponent component) where TComponent : struct =>
        GetCollection<TComponent>().Register(entity, component);

    /// <summary>
    /// Removes a component from an entity. 
    /// </summary>
    /// <inheritdoc cref="ComponentCollection.Unregister{TComponent}"/>
    public void RemoveComponent<TComponent>(Entity entity) where TComponent : struct =>
        GetCollection<TComponent>().Unregister<TComponent>(entity);
}
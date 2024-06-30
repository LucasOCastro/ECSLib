using ECSLib.Entities;
using ECSLib.Extensions;

namespace ECSLib.Archetypes;

internal class ArchetypeManager
{
    private readonly ArchetypeStorage _storage = new();
    private readonly Dictionary<Entity, int> _entityToArchetype = new();

    /// <summary>
    /// Registers an entity to the archetype storage with the default empty archetype.
    /// </summary>
    public void Register(Entity entity)
    {
        var archetype = _storage.GetOrCreateArchetype(Array.Empty<Type>());
        _entityToArchetype.Add(entity, archetype);
    }
    
    /// <summary>
    /// Unregisters an entity from the archetype storage.
    /// </summary>
    public void Unregister(Entity entity)
    {
        _entityToArchetype.Remove(entity);
    }
    
    /// <returns>The <see cref="Type"/> of every component an entity has in its archetype.</returns>
    public IEnumerable<Type> GetAllComponentTypes(Entity entity)
    {
        var archetype = _entityToArchetype[entity];
        return _storage.GetAllComponentsInArchetype(archetype);
    }
    
    /// <summary>
    /// Changes an entity's currently registered archetype to the one defined by the new components.
    /// </summary>
    private void ArchetypeUpdate(Entity entity, IEnumerable<Type> newComponents)
    {
        var newArchetype = _storage.GetOrCreateArchetype(newComponents);
        _entityToArchetype[entity] = newArchetype;
    }
    
    /// <summary>
    /// Updates an entity's archetype to another which includes all previous component types including the one that was added.
    /// </summary>
    public void BeforeComponentAddedTo(Entity entity, Type componentType)
    {
        var oldArchetype = _entityToArchetype[entity];
        var newComponents = _storage.GetAllComponentsInArchetype(oldArchetype).Append(componentType);
        ArchetypeUpdate(entity, newComponents);
    }

    /// <summary>
    /// Updates an entity's archetype to another which includes all previous component types except the one that was removed.
    /// </summary>
    public void BeforeComponentRemovedFrom(Entity entity, Type componentType)
    {
        var oldArchetype = _entityToArchetype[entity];
        var newComponents = _storage.GetAllComponentsInArchetype(oldArchetype).Except(componentType);
        ArchetypeUpdate(entity, newComponents);
    }
}
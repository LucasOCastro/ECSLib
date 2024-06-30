using ECSLib.Entities;
using ECSLib.Extensions;

namespace ECSLib.Archetypes;

internal class ArchetypeManager
{
    private readonly ArchetypeStorage _storage = new();
    private readonly Dictionary<Entity, int> _entityToArchetype = new();

    private void ArchetypeUpdate(Entity entity, IEnumerable<Type> newComponents)
    {
        var newArchetype = _storage.GetOrCreateArchetype(newComponents);
        _entityToArchetype[entity] = newArchetype;
    }
    
    public void BeforeComponentAddedTo(Type componentType, Entity entity)
    {
        var oldArchetype = _entityToArchetype[entity];
        var newComponents = _storage.GetAllComponentsInArchetype(oldArchetype).Append(componentType);
        ArchetypeUpdate(entity, newComponents);
    }

    public void BeforeComponentRemovedFrom(Type componentType, Entity entity)
    {
        var oldArchetype = _entityToArchetype[entity];
        var newComponents = _storage.GetAllComponentsInArchetype(oldArchetype).Except(componentType);
        ArchetypeUpdate(entity, newComponents);
    }

    public IEnumerable<Type> GetAllComponentTypes(Entity entity)
    {
        var archetype = _entityToArchetype[entity];
        return _storage.GetAllComponentsInArchetype(archetype);
    }

    public void Unregister(Entity entity)
    {
        _entityToArchetype.Remove(entity);
    }
}
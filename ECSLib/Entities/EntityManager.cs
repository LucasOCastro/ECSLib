namespace ECSLib.Entities;

public class EntityManager
{
    /// <summary>
    /// Stores the ids which have been freed on entity deletion and should be used next.
    /// </summary>
    private readonly Queue<int> _freedIds = new();
    private int _entityCount;

    private int GetNextId() => _freedIds.TryDequeue(out int index) ? index : _entityCount;
    
    public Entity CreateEntity()
    {
        int id = GetNextId();
        _entityCount++;
        return new(id);
    }
    
    public void RemoveEntity(Entity entity)
    {
        _freedIds.Enqueue(entity.ID);
        _entityCount--;
    }
}
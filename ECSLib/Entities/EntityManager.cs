namespace ECSLib.Entities;

internal class EntityManager
{
    /// <summary>
    /// Stores the ids which have been freed on entity deletion and should be used next.
    /// </summary>
    private readonly Queue<int> _freedIds = [];
    
    /// <summary>
    /// Maps each entity id to the current generation.
    /// </summary>
    private readonly List<int> _generations = [];


    public IEnumerable<Entity> AllEntities => _generations.Select((t, i) => new Entity(i, t));

    private int GetNextId() => _freedIds.TryDequeue(out int index) ? index : _generations.Count;
    
    public Entity CreateEntity()
    {
        int id = GetNextId();
        if (id >= _generations.Count)
            _generations.Add(0);
        return new(id, _generations[id]);
    }
    
    public void RemoveEntity(Entity entity)
    {
        _generations[entity.ID]++;
        _freedIds.Enqueue(entity.ID);
    }

    public bool IsValid(Entity entity) =>
        entity.ID < _generations.Count && entity.Generation == _generations[entity.ID];
}
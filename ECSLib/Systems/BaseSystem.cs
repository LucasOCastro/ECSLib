using ECSLib.Entities;

namespace ECSLib.Systems;

public abstract class BaseSystem
{
    protected abstract Query GetQuery();
    
    protected abstract void Process(float dt, ECS world, IEnumerable<Entity> entities);
    
    public void Process(float dt, ECS world)
    {
        Process(dt, world, world.Query(GetQuery()));
    }
}
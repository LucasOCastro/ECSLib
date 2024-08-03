using ECSLib.Entities;

namespace ECSLib.Components.Interning;

/// <summary>
/// Global context to be used by <see cref="RefPool{T}"/>.
/// Use <see cref="BeginContext"/> before registering anything in <see cref="RefPool{T}"/>,
/// then use <see cref="EndContext"/> afterwards.
/// </summary>
public static class RefPoolContext
{
    public class Context
    {
        private readonly List<Action> _unregisterCallbacks = [];

        public readonly Entity Entity;
        
        public readonly ECS World;
        
        public Context(Entity entity, ECS world)
        {
            Entity = entity;
            World = world;
        }
        
        public void OnEntityDestroyed(Entity entity)
        {
            if (entity != Entity) return;

            World.OnEntityDestroyed -= OnEntityDestroyed;
            foreach (var callback in _unregisterCallbacks)
            {
                callback();
            }
        }

        public void AddUnregisterCallback(Action action) => _unregisterCallbacks.Add(action);
    }

    public static Context? CurrentContext { get; private set; }

    public static void BeginContext(Entity entity, ECS world)
    {
        if (CurrentContext != null)
            throw new("Tried to begin RefPool context without ending previous context.");
        CurrentContext = new(entity, world);
        world.OnEntityDestroyed += CurrentContext.OnEntityDestroyed;
    }

    public static void EndContext(Entity entity, ECS world)
    {
        if (CurrentContext == null || entity != CurrentContext.Entity || world != CurrentContext.World)
            throw new("Tried to end RefPool context for an entity which is not the current context");
        CurrentContext = null;
    }
}
using ECSLib.Systems.Exceptions;

namespace ECSLib.Systems;

internal class SystemManager
{
    private readonly List<BaseSystem> _systems = [];
    private readonly HashSet<Type> _storedTypes = [];
    
    /// <summary>
    /// Registers a new system to be processed. 
    /// </summary>
    /// <param name="system">The system to be registered. Only one system of each type is allowed per world.</param>
    public void RegisterSystem(BaseSystem system)
    {
        if (!_storedTypes.Add(system.GetType()))
        {
            throw new RepeatedSystemException(system.GetType());
        }
        _systems.Add(system);
    }

    /// <summary>
    /// Updates all the registered systems.
    /// </summary>
    /// <param name="dt">Delta time elapsed from the last frame.</param>
    /// <param name="world">The ECS world the entities belong to.</param>
    public void Process(float dt, ECS world)
    {
        foreach (var system in _systems)
        {
            system.Process(dt, world);
        }
    }
}
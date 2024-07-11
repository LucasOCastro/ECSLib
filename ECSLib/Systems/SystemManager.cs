using ECSLib.Systems.Exceptions;

namespace ECSLib.Systems;

internal class SystemManager
{
    private readonly List<BaseSystem> _systems = [];
    private readonly HashSet<Type> _storedTypes = [];
    
    /// <summary> Registers a new system to be processed in <see cref="Process"/>.</summary>
    /// <param name="system">The system to be registered. Only one system of each type is allowed per world.</param>
    public void RegisterSystem(BaseSystem system)
    {
        if (!_storedTypes.Add(system.GetType()))
        {
            throw new RepeatedSystemException(system.GetType());
        }
        _systems.Add(system);
    }

    /// <summary> Registers a new system to be processed in <see cref="Process"/>.</summary>
    /// <typeparam name="T">The type of the system to be isntantiated and registered. Only one system of each type is allowed per world.</typeparam>
    public void RegisterSystem<T>() where T : BaseSystem, new() => RegisterSystem(new T());

    /// <summary>
    /// Updates all the registered systems.
    /// </summary>
    /// <param name="world">The ECS world the entities belong to.</param>
    public void Process(ECS world)
    {
        foreach (var system in _systems)
        {
            system.Process(world);
        }
    }
}
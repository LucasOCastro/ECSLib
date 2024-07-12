using ECSLib.Extensions;
using ECSLib.Systems.Exceptions;

namespace ECSLib.Systems;

internal class SystemManager
{
    private readonly SortedDictionary<int, List<BaseSystem>> _systems = [];
    private readonly HashSet<Type> _storedTypes = [];
    
    /// <summary> Registers a new system to be processed in <see cref="Process"/>.</summary>
    /// <param name="system">The system to be registered. Only one system of each type is allowed per world.</param>
    public void RegisterSystem(BaseSystem system)
    {
        if (!_storedTypes.Add(system.GetType()))
        {
            throw new RepeatedSystemException(system.GetType());
        }
        
        var list = _systems.GetOrAddNew(system.Pipeline);
        list.Add(system);
    }

    /// <summary> Registers a new system to be processed in <see cref="Process"/>.</summary>
    /// <typeparam name="T">The type of the system to be isntantiated and registered. Only one system of each type is allowed per world.</typeparam>
    public void RegisterSystem<T>() where T : BaseSystem, new() => RegisterSystem(new T());

    /// <summary>
    /// Processes all the registered systems, ordered by their pipeline index.
    /// </summary>
    /// <param name="world">The ECS world the entities belong to.</param>
    public void Process(ECS world)
    {
        foreach (var list in _systems.Values)
        {
            foreach (var system in list)
            {
                system.Process(world);
            }
        }
    }

    /// <summary>
    /// Processes the registered systems of a specific pipeline.
    /// </summary>
    public void Process(ECS world, int pipeline)
    {
        if (!_systems.TryGetValue(pipeline, out var list)) return;
        foreach (var system in list)
        {
            system.Process(world);
        }
    }
}
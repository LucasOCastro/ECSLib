using System.Reflection;
using ECSLib.Extensions;
using ECSLib.Systems.Attributes;
using ECSLib.Systems.Exceptions;

namespace ECSLib.Systems;

internal class SystemManager
{
    private readonly SortedDictionary<int, List<BaseSystem>> _systems = [];
    private readonly HashSet<Type> _storedTypes = [];
    private readonly HashSet<int> _blacklistedPipelines = [];

    private Type? _pipelineEnumType;
    public Type? PipelineEnumType
    {
        get => _pipelineEnumType;
        set
        {
            if (value is { IsEnum: false })
                throw new InvalidPipelineTypeException(value);
            _pipelineEnumType = value;
        }
    }

    /// <summary>
    /// Searches the assembly to find an enum type with the <see cref="PipelineEnumAttribute"/> attribute.
    /// </summary>
    public void RegisterPipelineEnumType(Assembly assembly)
    {
        PipelineEnumType = assembly.GetTypes()
            .FirstOrDefault(t => t.IsEnum && t.GetCustomAttribute<PipelineEnumAttribute>() != null);
        
        if (PipelineEnumType == null)
        {
            throw new MissingPipelineEnumTypeException();
        }
    }

    
    /// <summary> Searches the assembly for systems to register. </summary>
    public void RegisterAllSystems(Assembly assembly) 
    {
        foreach (var systemType in assembly.AllConcreteTypesWhichInherit(typeof(BaseSystem)))
        {
            var attribute = systemType.GetCustomAttribute<ECSSystemClassAttribute>();
            if (attribute == null || attribute.DoNotRegister) continue;
            try
            {
                var system = (BaseSystem)Activator.CreateInstance(systemType)!;
                RegisterSystem(system);
            }
            catch (MissingMethodException e)
            {
                Console.Error.Write(new MissingConstructorException(systemType, e));
            }
        }   
    }
    
    /// <returns>true if a pipeline value has been configured to be skipped during processing.</returns>
    /// <exception cref="InvalidPipelineValueException">
    /// Thrown if '<see cref="pipeline"/>' is not a valid value in the pipeline enum.
    /// </exception>
    private bool ShouldBeBlacklisted(int pipeline)
    {
        if (PipelineEnumType == null) return false;
        
        foreach (var o in Enum.GetValues(PipelineEnumType))
            if (o is Enum enumValue && Convert.ToInt32(enumValue) == pipeline)
                return PipelineEnumType.GetField(enumValue.ToString())!.GetCustomAttribute<DoNotProcessAttribute>() != null;
        throw new InvalidPipelineValueException(PipelineEnumType, pipeline);
    }
    
    /// <summary> Registers a new system to be processed in <see cref="Process(ECS)"/>.</summary>
    /// <param name="system">The system to be registered. Only one system of each type is allowed per world.</param>
    public void RegisterSystem(BaseSystem system)
    {
        if (!_storedTypes.Add(system.GetType()))
        {
            throw new RepeatedSystemException(system.GetType());
        }
        
        if (!_systems.TryGetValue(system.Pipeline, out var list))
        {
            list = [];
            _systems.Add(system.Pipeline, list);
            if (ShouldBeBlacklisted(system.Pipeline)) 
                _blacklistedPipelines.Add(system.Pipeline);
        }
        list.Add(system);
    }

    /// <summary> Registers a new system to be processed in <see cref="Process(ECS)"/>.</summary>
    /// <typeparam name="T">The type of the system to be instantiated and registered. Only one system of each type is allowed per world.</typeparam>
    public void RegisterSystem<T>() where T : BaseSystem, new() => RegisterSystem(new T());

    /// <summary>
    /// Processes all the registered systems, ordered by their pipeline index.
    /// </summary>
    /// <param name="world">The ECS world the entities belong to.</param>
    public void Process(ECS world)
    {
        foreach (var pair in _systems)
        {
            if (_blacklistedPipelines.Contains(pair.Key)) continue;
            foreach (var system in pair.Value)
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

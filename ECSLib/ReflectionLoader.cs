using System.Reflection;
using ECSLib.Systems;
using ECSLib.Systems.Exceptions;

namespace ECSLib;

internal static class ReflectionLoader
{
    private static IEnumerable<Type> AllConcreteTypesWhichInherit(Type baseType)
    {
        var assembly = Assembly.GetEntryAssembly();
        return assembly != null ? assembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)) : [];
    }
    
    
    /// <summary>
    /// Registers all concrete classes which inherit <see cref="BaseSystem"/> into the provided <see cref="SystemManager"/>.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown if wasn't able to load the entry assembly.</exception>
    public static void RegisterAllSystems(SystemManager systemManager)
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
        {
            throw new NullReferenceException("Null assembly.");
        }

        foreach (var systemType in AllConcreteTypesWhichInherit(typeof(BaseSystem)))
        {
            try
            {
                var system = (BaseSystem)Activator.CreateInstance(systemType)!;
                systemManager.RegisterSystem(system);
            }
            catch (MissingMethodException e)
            {
                Console.Error.Write(new MissingConstructorException(systemType, e));
            }
        }
    }
}
using System.Reflection;
using ECSLib.Systems;
using ECSLib.Systems.Exceptions;

namespace ECSLib;

internal static class ReflectionLoader
{
    private static IEnumerable<Type> AllConcreteTypesWhichInherit(IEnumerable<Assembly> assemblies, Type baseType)
    {
        return assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t));
    }

    private static IEnumerable<Assembly> GetAllAssembliesFrom(Assembly baseAssembly)
    {
        yield return baseAssembly;
        foreach (var referencedAssembly in baseAssembly.GetReferencedAssemblies())
        {
            yield return Assembly.Load(referencedAssembly);
        }
    }
    
    
    /// <summary>
    /// Registers all concrete classes which inherit <see cref="BaseSystem"/> into the provided <see cref="SystemManager"/>.
    /// </summary>
    /// <exception cref="NullReferenceException">Thrown if wasn't able to load the entry assembly.</exception>
    public static void RegisterAllSystems(SystemManager systemManager, Assembly assembly)
    {
        foreach (var systemType in AllConcreteTypesWhichInherit(GetAllAssembliesFrom(assembly), typeof(BaseSystem)))
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
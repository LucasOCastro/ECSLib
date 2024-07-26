using System.Reflection;

namespace ECSLib.Extensions;

public static class ReflectionUtility
{
    public static IEnumerable<Type> AllConcreteTypesWhichInherit(this Assembly assembly, Type baseType)
    {
        return assembly.GetTypes()
            .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t));
    }
}
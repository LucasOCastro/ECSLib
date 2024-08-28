using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ECSLib.Extensions;

public static class ReflectionUtility
{
    public static IEnumerable<Type> AllConcreteTypesWhichInherit(this Assembly assembly, Type baseType) =>
        assembly.GetTypes()
            .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t));

    public static bool GenericDefinitionEquals(this Type type, Type genericDefinition) =>
        type.IsConstructedGenericType && type.GetGenericTypeDefinition() == genericDefinition;
    
    /// <example>
    /// Dictionary&lt;int, string&gt; inherits from IDictionary&lt;,&gt;<br/>
    /// List&lt;int&gt; inherits from IList&lt;&gt; and ICollection&lt;&gt;
    /// </example>
    public static bool ImplementsGenericInterface(this Type type, Type genericType, [NotNullWhen(true)] out Type? constructedInterface)
    {
        constructedInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == genericType);
        return constructedInterface != null;
    }

    public static bool ImplementsGenericInterface(this Type type, Type genericType) =>
        type.ImplementsGenericInterface(genericType, out _);
}
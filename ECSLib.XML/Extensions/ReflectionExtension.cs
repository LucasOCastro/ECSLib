using System.Diagnostics.Contracts;
using System.Reflection;

namespace ECSLib.XML.Extensions;

internal static class ReflectionExtension
{
    [Pure]
    public static MemberInfo? GetFieldOrProperty(this Type type, string name, BindingFlags bindingFlags)
    {
        return (MemberInfo?)type.GetField(name, bindingFlags) ?? type.GetProperty(name, bindingFlags);
    }

    [Pure]
    public static Type GetFieldOrPropertyType(this MemberInfo member) =>
        member switch
        {
            FieldInfo field => field.FieldType,
            PropertyInfo property => property.PropertyType,
            _ => throw new ArgumentException("Member is neither field or property.")
        };

    public static bool IsCollection(this Type type)
    {
        var args = type.GetGenericArguments();
        if (args.Length == 1 && typeof(ICollection<>).MakeGenericType(args).IsAssignableFrom(type))
            return true;
        return type.BaseType != null && type.BaseType.IsCollection();
    }

    /// <summary>
    /// For example, 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="genericType"></param>
    /// <returns></returns>
    /// <example>
    /// Dictionary&lt;int, string&gt; inherits from IDictionary&lt;,&gt;<br/>
    /// List&lt;int&gt; inherits from IList&lt;&gt; and ICollection&lt;&gt;
    /// </example>
    public static bool ImplementsGenericInterface(this Type type, Type genericType)
    {
        return type.GetInterfaces().Any(i => i.IsConstructedGenericType && i.GetGenericTypeDefinition() == genericType);
    }
    
}
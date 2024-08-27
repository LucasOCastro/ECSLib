using System.Diagnostics.Contracts;
using System.Reflection;
using ECSLib.Extensions;

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

    public static bool IsCollection(this Type type) => type.ImplementsGenericInterface(typeof(ICollection<>));
}
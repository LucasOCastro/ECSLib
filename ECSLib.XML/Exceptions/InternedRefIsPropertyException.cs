using System.Reflection;
using ECSLib.Components.Interning;

namespace ECSLib.XML.Exceptions;

public class InternedRefIsPropertyException(MemberInfo member)
    : Exception($"{member.Name} is of type {typeof(PooledRef<>)} but is not a Field.");
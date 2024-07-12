using System.Reflection;
using ECSLib.Components;
using ECSLib.Entities;

namespace ECSLib.Systems.Attributes;

public class InvalidComponentRefTypeException(Type type) : Exception($"{type.Name} is not a valid component reference type.");

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ECSSystemAttribute : Attribute
{
    public Type[]? All;
    public Type[]? Any;
    public Type[]? None;

    public Query GenQuery(IEnumerable<ParameterInfo> parameters)
    {
        var all = All?.ToList() ?? [];
        var any = Any?.ToList() ?? [];
        var none = None?.ToList() ?? [];
        foreach (var parameter in parameters)
        {
            var type = parameter.ParameterType;
            if (type == typeof(Entity) || type == typeof(ECS)) continue;
            if (!type.IsByRef) throw new InvalidComponentRefTypeException(type);

            //Extract Comp<T> from ref Comp<T>
            //Extract T from ref T
            type = type.GetElementType()!;
            
            //Extract T from Comp<T>
            if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Comp<>))
            {
                type = type.GetGenericArguments()[0];
            }
            
            //TODO Study how to accept (a || b) && (c || d) pattern
            if (parameter.GetCustomAttribute<AnyAttribute>() != null)
                any.Add(type);
            else 
                all.Add(type);
        }

        return Query
            .With(all.ToArray())
            .WithAny(any.ToArray())
            .WithNone(none.ToArray());
    }
}

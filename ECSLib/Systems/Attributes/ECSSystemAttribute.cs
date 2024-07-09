using System.Reflection;
using ECSLib.Components;
using ECSLib.Entities;

namespace ECSLib.Systems.Attributes;

public class InvalidComponentRefTypeException(Type type) : Exception($"{type.Name} is not a valid component reference type.");

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class ECSSystemAttribute : Attribute
{
    private readonly Type[]? _all;
    private readonly Type[]? _any;
    private readonly Type[]? _none;
     
    // See the attribute guidelines at 
    //  http://go.microsoft.com/fwlink/?LinkId=85236
    public ECSSystemAttribute(Type[]? with = null, Type[]? any = null, Type[]? none = null)
    {
        
    }

    public Query GenQuery(IEnumerable<ParameterInfo> parameters)
    {
        var all = _all?.ToList() ?? [];
        var any = _any?.ToList() ?? [];
        var none = _none?.ToList() ?? [];
        foreach (var parameter in parameters)
        {
            var byRefType = parameter.ParameterType;
            if (byRefType == typeof(Entity) || byRefType == typeof(ECS)) continue;
            if (!byRefType.IsByRef) throw new InvalidComponentRefTypeException(byRefType);
            
            var compRefType = byRefType.GetElementType()!;
            if (!compRefType.IsConstructedGenericType || compRefType.GetGenericTypeDefinition() != typeof(Comp<>)) 
                throw new InvalidComponentRefTypeException(compRefType);

            var type = compRefType.GetGenericArguments()[0];
            if (parameter.GetCustomAttribute<AnyAttribute>() != null)
                any.Add(type);
            else all.Add(type);
        }

        return Query
            .With(all.ToArray())
            .WithAny(any.ToArray())
            .WithNone(none.ToArray());
    }
}

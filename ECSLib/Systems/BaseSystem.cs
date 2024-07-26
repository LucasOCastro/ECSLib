using System.Reflection;
using ECSLib.Systems.Attributes;

namespace ECSLib.Systems;

public abstract class BaseSystem
{
    protected static Query GenQueryForMethod(Type type, string methodName)
    {
        var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null) throw new MissingMethodException(type.Name, methodName);
        var attribute = method.GetCustomAttribute<ECSSystemAttribute>();
        if (attribute == null) throw new CustomAttributeFormatException();
        return attribute.GenQuery(method.GetParameters());
    }
    
    public abstract void Process(ECS world);

    private int? _pipelineIndex;

    public int Pipeline
    {
        get
        {
            _pipelineIndex ??= GetType().GetCustomAttribute<ECSSystemClassAttribute>()?.Pipeline ?? 0;
            return _pipelineIndex.Value;
        }
    }
}
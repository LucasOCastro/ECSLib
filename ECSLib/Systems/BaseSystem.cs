using System.Reflection;
using ECSLib.Systems.Attributes;

namespace ECSLib.Systems;

public abstract partial class BaseSystem
{
    protected static Query GenQueryForMethod(Type type, string methodName)
    {
        var method = type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (method == null) throw new MissingMethodException(type.Name, methodName);
        var attribute = method.GetCustomAttribute<ECSSystemAttribute>();
        if (attribute == null) throw new CustomAttributeFormatException();
        return attribute.GenQuery(method.GetParameters());
    }
    
    public abstract void Process(ECS world);

    /*private readonly record struct SystemRecord(Query Query, Delegate Delegate);
    private SystemRecord[] _records = [];
    private void RegisterAllMethods()
    {
        var methods = GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        List<SystemRecord> records = [];
        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<ECSSystemAttribute>();
            if (attribute == null) continue;

            Delegate funcDelegate = default;// method.deleg;
            records.Add(new(attribute.GenQuery(method.GetParameters()), funcDelegate ));
        }

        _records = records.ToArray();
    }*/
}
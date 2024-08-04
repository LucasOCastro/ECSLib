using System.Reflection.Emit;

namespace ECSLib.XML.ValueEmitters;

internal class DictionaryValueEmitter : IMergeableValueEmitter
{
    private readonly List<KeyValuePair<IValueEmitter, IValueEmitter>> _items;

    public DictionaryValueEmitter(List<KeyValuePair<IValueEmitter, IValueEmitter>> items)
    {
        _items = items;
    }
    
    public void Emit(ILGenerator il, Type type)
    {
        var keyType = type.GetGenericArguments()[0];
        var valueType = type.GetGenericArguments()[1];
        var addMethod = type.GetMethod(nameof(IDictionary<int, int>.Add), [keyType, valueType]);
        var ctor = type.GetConstructor(Type.EmptyTypes);
        if (addMethod == null || ctor == null) throw new MissingMethodException("Missing methods for dictionary emission.");
        il.Emit(OpCodes.Newobj, ctor);

        foreach (var item in _items)
        {
            il.Emit(OpCodes.Dup);
            item.Key.Emit(il, keyType);
            item.Value.Emit(il, valueType);
            il.Emit(OpCodes.Call, addMethod);
            if (addMethod.ReturnType != null && addMethod.ReturnType != typeof(void)) 
                il.Emit(OpCodes.Pop);
        }   
    }

    public void MergeWith(IValueEmitter other)
    {
        if (other is not DictionaryValueEmitter otherDict) return;
        _items.AddRange(otherDict._items);
    }
}
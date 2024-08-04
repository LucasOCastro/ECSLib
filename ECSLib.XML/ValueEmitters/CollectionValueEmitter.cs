 using System.Reflection.Emit;

 namespace ECSLib.XML.ValueEmitters;

internal class CollectionValueEmitter : IMergeableValueEmitter
{
    private readonly List<IValueEmitter> _items;

    public CollectionValueEmitter(List<IValueEmitter> items)
    {
        _items = items;
    }

    private void EmitAsArray(ILGenerator il, Type itemType)
    {
        il.Emit(OpCodes.Ldc_I4, _items.Count);
        il.Emit(OpCodes.Newarr, itemType);

        for (int i = 0; i < _items.Count; i++)
        {
            il.Emit(OpCodes.Dup);
            il.Emit(OpCodes.Ldc_I4, i);
            _items[i].Emit(il, itemType);
            il.Emit(OpCodes.Stelem, itemType);
        }
    }

    private void EmitAsICollection(ILGenerator il, Type collectionType, Type itemType)
    {
        var addMethod = collectionType.GetMethod(nameof(ICollection<int>.Add), [itemType]);
        var ctor = collectionType.GetConstructor(Type.EmptyTypes);
        if (addMethod == null || ctor == null) throw new MissingMethodException("Missing methods for collection emission.");
        il.Emit(OpCodes.Newobj, ctor);

        foreach (var item in _items)
        {
            il.Emit(OpCodes.Dup);
            item.Emit(il, itemType);
            il.Emit(OpCodes.Call, addMethod);
            if (addMethod.ReturnType != null && addMethod.ReturnType != typeof(void)) 
                il.Emit(OpCodes.Pop);
        }
    }

    public void Emit(ILGenerator il, Type type)
    {
        if (type.IsArray) EmitAsArray(il, type.GetElementType()!);
        else EmitAsICollection(il, type, type.GetGenericArguments()[0]);
    }

    public void MergeWith(IValueEmitter other)
    {
        if (other is not CollectionValueEmitter otherCol) return;
        _items.AddRange(otherCol._items);
    }
    
    public IValueEmitter Copy() => new CollectionValueEmitter([.._items]);
}
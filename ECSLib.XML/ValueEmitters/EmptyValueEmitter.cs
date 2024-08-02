using System.Reflection.Emit;
using ECSLib.XML.Extensions;

namespace ECSLib.XML.ValueEmitters;

internal class EmptyValueEmitter : IValueEmitter
{
    public void Emit(ILGenerator il, Type type)
    {
        //If boolean tag is present, load as true
        if (type == typeof(bool))
        {
            il.Emit(OpCodes.Ldc_I4_1);
            return;
        }

        if (type.IsArray || type.IsCollection())
        {
            CollectionValueEmitter collectionEmitter = new([]);
            collectionEmitter.Emit(il, type);
            return;
        }

        if (!type.IsByRef)
        {
            StructValueEmitter structEmitter = new([]);
            structEmitter.Emit(il, type);
            return;
        }

        throw new NotSupportedException($"{type.FullName} is not a supported type for empty XML tags.");
    }
}
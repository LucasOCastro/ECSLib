using System.Reflection.Emit;

namespace ECSLib.XML.ValueEmitters;

internal interface IValueEmitter
{
    void Emit(ILGenerator il, Type type);
}
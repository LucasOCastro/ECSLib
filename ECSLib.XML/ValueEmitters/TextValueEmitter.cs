using System.Globalization;
using System.Reflection.Emit;
using ECSLib.XML.Extensions;

namespace ECSLib.XML.ValueEmitters;

internal class TextValueEmitter : IValueEmitter
{
    private readonly string _text;
    public TextValueEmitter(string text)
    {
        _text = text;
    }

    public void Emit(ILGenerator il, Type type)
    {
        var value = Convert.ChangeType(_text, type, CultureInfo.InvariantCulture);
        il.EmitLoadConstant(value);
    }
    
    public IValueEmitter Copy() => new TextValueEmitter(_text);
}
using System.Reflection.Emit;
using ECSLib.XML.Extensions;
using ECSLib.XML.Parsing;

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
        il.EmitLoadConstant(StringParserManager.Parse(_text, type));
    }
    
    public IValueEmitter Copy() => new TextValueEmitter(_text);
}
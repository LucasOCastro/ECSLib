using System.Globalization;
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
        var constructorParser = StringParserManager.TryGetConstructorParserForType(type);
        if (constructorParser != null)
        {
            var parsed = constructorParser.Parse(_text, type);
            new StructValueEmitter(parsed).Emit(il, type);
            return;
        }

        il.EmitLoadConstant(Convert.ChangeType(_text, type, CultureInfo.InvariantCulture));
    }
    
    public IValueEmitter Copy() => new TextValueEmitter(_text);
}
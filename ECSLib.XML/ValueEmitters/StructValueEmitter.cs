using System.Reflection;
using System.Reflection.Emit;
using ECSLib.Components.Interning;
using ECSLib.XML.Exceptions;
using ECSLib.XML.Extensions;
using ECSLib.XML.Parsing;

namespace ECSLib.XML.ValueEmitters;

internal class StructValueEmitter : IValueEmitter
{
    public LocalBuilder? ComponentLocal { get; private set; }
    
    private readonly ConstructorInfo? _constructor;
    
    private readonly List<IValueEmitter> _constructorArgs;
    
    private readonly Dictionary<string, IValueEmitter> _fields;
    
    public StructValueEmitter(ConstructorInfo? constructor, List<IValueEmitter> constructorArgs,
        Dictionary<string, IValueEmitter> fields)
    {
        _constructor = constructor;
        _constructorArgs = constructorArgs;
        _fields = fields;
    }

    public StructValueEmitter(ParsedConstructor parsedConstructor)
    {
        _constructor = parsedConstructor.Constructor;
        _constructorArgs = parsedConstructor.ConstructorArgs
            .Select(a => (IValueEmitter)new TextValueEmitter(a))
            .ToList();
        _fields = parsedConstructor.Fields
            .ToDictionary(f => f.Key,
                f => (IValueEmitter)new TextValueEmitter(f.Value)
            );
    }

    private void EmitConstructorArgs(ILGenerator generator)
    {
        if (_constructor == null || _constructor.GetParameters().Length != _constructorArgs.Count)
            return;
        
        var parameters = _constructor.GetParameters();
        for (int i = 0; i < parameters.Length; i++)
            _constructorArgs[i].Emit(generator, parameters[i].ParameterType);
    }
    
    public void Emit(ILGenerator generator, Type type)
    {
        ComponentLocal = _constructor == null
            ? generator.EmitStructConstructor(type)
            : generator.EmitStructConstructor(type, _constructor, EmitConstructorArgs);
        foreach (var (fieldName, fieldValueEmitter) in  _fields)
        {
            var member = type.GetFieldOrProperty(fieldName, BindingFlags.Instance | BindingFlags.Public);
            if (member == null) throw new MissingMemberException(type.Name, fieldName);
                
            //Load the component's address onto the stack
            generator.Emit(OpCodes.Ldloca_S, ComponentLocal);
                
            Type valueType = member.GetFieldOrPropertyType();
            //If the field is an interned ref, load the interning struct address onto the stack
            if (valueType.IsConstructedGenericType && valueType.GetGenericTypeDefinition() == typeof(PooledRef<>))
            {
                var valueProp = valueType.GetProperty("Value", BindingFlags.Instance | BindingFlags.Public)!;
                if (member is FieldInfo f) generator.Emit(OpCodes.Ldflda, f);
                else throw new InternedRefIsPropertyException(member);
                    
                valueType = valueType.GenericTypeArguments[0];
                member = valueProp;
            }
                
            //Load the actual value into the stack
            fieldValueEmitter.Emit(generator, valueType);

            switch (member)
            {
                case FieldInfo field:
                    generator.Emit(OpCodes.Stfld, field);
                    break;
                case PropertyInfo prop:
                    if (prop.SetMethod == null)
                        throw new MissingMethodException(
                            $"{valueType.Name} has deserialized prop {prop.Name} with no setter.");
                    generator.Emit(OpCodes.Call, prop.SetMethod);
                    break;
            }
        }

        //Copy the struct onto the stack to return
        generator.Emit(OpCodes.Ldloc, ComponentLocal);
    }
    
    public IValueEmitter Copy() => new StructValueEmitter(_constructor, [.._constructorArgs], new(_fields));
}
using System.Reflection;
using System.Reflection.Emit;
using ECSLib.Components.Interning;
using ECSLib.XML.Exceptions;
using ECSLib.XML.Extensions;

namespace ECSLib.XML.ValueEmitters;

internal class StructValueEmitter : IValueEmitter
{
    private readonly Dictionary<string, IValueEmitter> _fields;
    public StructValueEmitter(Dictionary<string, IValueEmitter> fields)
    {
        _fields = fields;
    }

    public LocalBuilder? ComponentLocal { get; private set; }
    
    public void Emit(ILGenerator generator, Type type)
    {
        ComponentLocal = generator.EmitStructConstructor(type);
        foreach (var (fieldName, fieldValueEmitter) in _fields)
        {
            var member = type.GetFieldOrProperty(fieldName, BindingFlags.Instance | BindingFlags.Public);
            if (member == null) throw new MissingMemberException(type.Name, fieldName);
                
            //Load the component's address onto the stack
            generator.Emit(OpCodes.Ldloca_S, ComponentLocal);
                
            Type valueType = member.GetFieldOrPropertyType();
            //If the field is an interned ref, load the interning struct address onto the stack
            if (valueType.IsConstructedGenericType && valueType.GetGenericTypeDefinition() == typeof(PooledRef<>))
            {
                var valueProp = valueType.GetProperty(nameof(PooledRef<object>.Value), BindingFlags.Instance | BindingFlags.Public)!;
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
                    generator.Emit(OpCodes.Call, prop.SetMethod);
                    break;
            }
        }

        //Copy the struct onto the stack to return
        generator.Emit(OpCodes.Ldloc, ComponentLocal);
    }
    
    public IValueEmitter Copy() => new StructValueEmitter(new(_fields));
}
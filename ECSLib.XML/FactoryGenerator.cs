using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;
using ECSLib.Components.Interning;
using ECSLib.Entities;
using ECSLib.XML.Exceptions;
using ECSLib.XML.Extensions;

namespace ECSLib.XML;

internal static class FactoryGenerator
{
    private static readonly MethodInfo CreateEntityMethodInfo =
        typeof(ECS).GetMethod(nameof(ECS.CreateEntity), BindingFlags.Instance | BindingFlags.Public)!;

    private static readonly MethodInfo AddComponentGenericMethodInfo =
        typeof(ECS).GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(m => m.Name == nameof(ECS.AddComponent) && m.GetParameters().Length == 2);

    private static object ConvertString(string value, Type type)
    {
        //Can simply set to true by including <booleanField/>
        if (type == typeof(bool) && string.IsNullOrEmpty(value))
            return true;
        
        return Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
    }
    
    public static EntityFactoryDelegate CreateEntityFactory(EntityModel model, Assembly assembly)
    {
        DynamicMethod dynamicMethod = new(
            name: model.Name,
            returnType: typeof(Entity),
            parameterTypes: [typeof(ECS)],
            m: typeof(FactoryGenerator).Module
        );
        var generator = dynamicMethod.GetILGenerator();

        //Entity entity = world.CreateEntity();
        var entityLocal = generator.DeclareLocal(typeof(Entity));
        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Call, CreateEntityMethodInfo);
        generator.Emit(OpCodes.Stloc, entityLocal);

        foreach (var (componentTypeName, fields) in model.Components)
        {
            var componentType = assembly.GetType(componentTypeName) ?? throw new InvalidComponentTypeNameException(componentTypeName, assembly);
            
            //Constructs and stores the component instance
            var componentLocal = generator.EmitStructConstructor(componentType);
            foreach (var (fieldName, fieldValue) in fields)
            {
                var member = componentType.GetFieldOrProperty(fieldName, BindingFlags.Instance | BindingFlags.Public);
                if (member == null) throw new MissingMemberException(componentType.Name, fieldName);
                
                //Load the component's address onto the stack
                generator.Emit(OpCodes.Ldloca_S, componentLocal);
                
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
                var value = ConvertString(fieldValue, valueType);
                generator.EmitLoadConstant(value);
                
                
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
         
            //TODO adding one comp per comp is not very efficient
            //ecs.AddComponent(entity, component);
            var addComponentMethodInfo = AddComponentGenericMethodInfo.MakeGenericMethod(componentType);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldloc, entityLocal);
            generator.Emit(OpCodes.Ldloc, componentLocal);
            generator.Emit(OpCodes.Call, addComponentMethodInfo);
        }

        generator.Emit(OpCodes.Ldloc, entityLocal);
        generator.Emit(OpCodes.Ret);
        return dynamicMethod.CreateDelegate<EntityFactoryDelegate>();
    }
}
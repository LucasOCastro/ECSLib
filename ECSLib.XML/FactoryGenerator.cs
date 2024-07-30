using System.Reflection;
using System.Reflection.Emit;
using ECSLib.Entities;
using ECSLib.XML.Exceptions;
using ECSLib.XML.Extensions;

namespace ECSLib.XML;

internal static class FactoryGenerator
{
    
    private static readonly MethodInfo CreateEntityMethodInfo =
        typeof(ECS).GetMethod(nameof(ECS.CreateEntity), BindingFlags.Instance | BindingFlags.Public)!;

    private static readonly MethodInfo AddComponentGenericMethodInfo =
        typeof(ECS).GetMethod(nameof(ECS.AddComponent), BindingFlags.Instance | BindingFlags.Public)!;
    
    public static EntityFactoryDelegate CreateEntityFactory(EntityModel model)
    {
        DynamicMethod dynamicMethod = new(model.Name, typeof(Entity), [typeof(ECS)]);
        var generator = dynamicMethod.GetILGenerator();

        //Entity entity = world.CreateEntity();
        var entityLocal = generator.DeclareLocal(typeof(Entity));
        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Calli, CreateEntityMethodInfo);
        generator.Emit(OpCodes.Stloc, entityLocal.LocalIndex);

        foreach (var (componentTypeName, fields) in model.Components)
        {
            var componentType = Type.GetType(componentTypeName) ?? throw new InvalidComponentTypeNameException(componentTypeName);
            
            //Constructs and stores the component instance
            var componentLocal = generator.EmitStructConstructor(componentType);
            foreach (var (fieldName, fieldValue) in fields)
            {
                var member = componentType.GetFieldOrProperty(fieldName, BindingFlags.Instance | BindingFlags.Public);
                if (member == null) throw new MissingMemberException(componentType.Name, fieldName);
                
                Type valueType = member.GetFieldOrPropertyType();
                var value = Convert.ChangeType(fieldValue, valueType);
                
                //Set the field/property
                generator.Emit(OpCodes.Ldloc, componentLocal.LocalIndex);
                generator.EmitLoadConstant(value);
                switch (member)
                {
                    case FieldInfo field:
                        generator.Emit(OpCodes.Stfld, field);
                        break;
                    case PropertyInfo prop:
                        generator.Emit(OpCodes.Calli, prop.SetMethod);
                        break;
                }
            }
         
            //TODO adding one comp per comp is not very efficient
            //ecs.AddComponent(entity, component);
            var addComponentMethodInfo = AddComponentGenericMethodInfo.MakeGenericMethod(componentType);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldloc, entityLocal.LocalIndex);
            generator.Emit(OpCodes.Ldloc, componentLocal.LocalIndex);
            generator.Emit(OpCodes.Calli, addComponentMethodInfo);
        }

        return dynamicMethod.CreateDelegate<EntityFactoryDelegate>();
    }
}
using System.Reflection;
using System.Reflection.Emit;
using System.Xml;
using ECSLib.Entities;
using ECSLib.XML.Exceptions;
using ECSLib.XML.Extensions;

namespace ECSLib.XML;

public static class EntityDeserializer
{
    private static readonly MethodInfo CreateEntityMethodInfo =
        typeof(ECS).GetMethod(nameof(ECS.CreateEntity), BindingFlags.Instance | BindingFlags.Public)!;

    private static readonly MethodInfo AddComponentGenericMethodInfo =
        typeof(ECS).GetMethod(nameof(ECS.AddComponent), BindingFlags.Instance | BindingFlags.Public)!;
    
    public static EntityFactoryDelegate CreateEntityFactory(XmlNode entityXml)
    {
        DynamicMethod dynamicMethod = new(entityXml.Name, typeof(Entity), [typeof(ECS)]);
        var generator = dynamicMethod.GetILGenerator();
        
        //Create and store entity
        var entityLocal = generator.DeclareLocal(typeof(Entity));
        generator.Emit(OpCodes.Ldarg_0);
        generator.Emit(OpCodes.Calli, CreateEntityMethodInfo);
        generator.Emit(OpCodes.Stloc, entityLocal.LocalIndex);
        
        //
        foreach (XmlNode componentNode in entityXml.ChildNodes)
        {
            //TODO This won't access assemblies correctly.
            var componentType = Type.GetType(componentNode.Name) ?? throw new InvalidComponentTypeNameException(entityXml.Name);
            
            //Constructs and stores the component instance
            var componentLocal = generator.DeclareLocal(componentType);
            generator.Emit(OpCodes.Newobj, componentType);
            generator.Emit(OpCodes.Stloc, componentLocal.LocalIndex);
            
            //
            foreach (XmlNode fieldXml in componentNode.ChildNodes)
            {
                var member = componentType.GetFieldOrProperty(fieldXml.Name, BindingFlags.Instance | BindingFlags.Public);
                if (member == null) throw new MissingMemberException(componentType.Name, fieldXml.Name);

                //Parse the default value
                var valueString = fieldXml.Value;
                Type valueType = member.GetFieldOrPropertyType();
                var value = Convert.ChangeType(valueString, valueType);
                
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
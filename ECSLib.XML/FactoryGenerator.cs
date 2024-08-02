using System.Reflection;
using System.Reflection.Emit;
using ECSLib.Entities;
using ECSLib.XML.Exceptions;
using ECSLib.XML.ValueEmitters;

namespace ECSLib.XML;

internal static class FactoryGenerator
{
    private static readonly MethodInfo CreateEntityMethodInfo =
        typeof(ECS).GetMethod(nameof(ECS.CreateEntity), BindingFlags.Instance | BindingFlags.Public)!;

    private static readonly MethodInfo AddComponentGenericMethodInfo =
        typeof(ECS).GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(m => m.Name == nameof(ECS.AddComponent) && m.GetParameters().Length == 2);

    public static EntityFactoryDelegate CreateEntityFactory(EntityModel model, Assembly assembly)
    {
        DynamicMethod dynamicMethod = new(
            name: model.Name + "Factory",
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
            StructValueEmitter compEmitter = new(fields);
            compEmitter.Emit(generator, componentType);
            generator.Emit(OpCodes.Pop);
         
            //TODO adding one comp per comp is not very efficient
            //ecs.AddComponent(entity, component);
            var addComponentMethodInfo = AddComponentGenericMethodInfo.MakeGenericMethod(componentType);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldloc, entityLocal);
            generator.Emit(OpCodes.Ldloc, compEmitter.ComponentLocal!);
            generator.Emit(OpCodes.Call, addComponentMethodInfo);
        }

        generator.Emit(OpCodes.Ldloc, entityLocal);
        generator.Emit(OpCodes.Ret);
        return dynamicMethod.CreateDelegate<EntityFactoryDelegate>();
    }
}
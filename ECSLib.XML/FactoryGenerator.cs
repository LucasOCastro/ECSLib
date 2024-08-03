using System.Reflection;
using System.Reflection.Emit;
using ECSLib.Components.Interning;
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

    private static readonly MethodInfo BeginContextMethodInfo =
        typeof(RefPoolContext).GetMethod(nameof(RefPoolContext.BeginContext),
            BindingFlags.Static | BindingFlags.Public, [typeof(Entity), typeof(ECS)])!;
    
    private static readonly MethodInfo EndContextMethodInfo =
        typeof(RefPoolContext).GetMethod(nameof(RefPoolContext.EndContext),
            BindingFlags.Static | BindingFlags.Public, [typeof(Entity), typeof(ECS)])!;

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

        var components = model.Components.Select(pair => (
            Type: assembly.GetType(pair.Key) ?? throw new InvalidComponentTypeNameException(pair.Key, assembly),
            Fields: pair.Value
        )).ToList();
        
        bool hasInternedFields = components.Any(c => InternedComponentTypesCache.HasInternedField(c.Type));
        if (hasInternedFields)
        {
            generator.Emit(OpCodes.Ldloc, entityLocal);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, BeginContextMethodInfo);
        }

        foreach (var (componentType, fields) in components)
        {
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

        if (hasInternedFields)
        {
            generator.Emit(OpCodes.Ldloc, entityLocal);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, EndContextMethodInfo);
        }
        
        generator.Emit(OpCodes.Ldloc, entityLocal);
        generator.Emit(OpCodes.Ret);
        return dynamicMethod.CreateDelegate<EntityFactoryDelegate>();
    }
}
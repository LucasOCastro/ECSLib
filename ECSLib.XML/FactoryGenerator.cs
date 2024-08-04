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
        typeof(ECS).GetMethod(nameof(ECS.CreateEntityWithComponents), 
            BindingFlags.Instance | BindingFlags.Public, [typeof(IEnumerable<Type>)])!;

    private static readonly MethodInfo SetComponentGenericMethodInfo =
        typeof(ECS).GetMethod(nameof(ECS.SetComponent), BindingFlags.Instance | BindingFlags.Public)!;

    private static readonly MethodInfo BeginContextMethodInfo =
        typeof(RefPoolContext).GetMethod(nameof(RefPoolContext.BeginContext),
            BindingFlags.Static | BindingFlags.Public, [typeof(Entity), typeof(ECS)])!;
    
    private static readonly MethodInfo EndContextMethodInfo =
        typeof(RefPoolContext).GetMethod(nameof(RefPoolContext.EndContext),
            BindingFlags.Static | BindingFlags.Public, [typeof(Entity), typeof(ECS)])!;

    private static readonly MethodInfo TypeFromHandleMethodInfo =
        typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle), BindingFlags.Static | BindingFlags.Public)!;
    
    public static EntityFactoryDelegate CreateEntityFactory(EntityModel model, Assembly assembly)
    {
        var components = model.Components.Select(pair => (
            Type: assembly.GetType(pair.Key) ?? throw new InvalidComponentTypeNameException(pair.Key, assembly),
            Fields: pair.Value
        )).ToList();
        
        DynamicMethod dynamicMethod = new(
            name: model.Name + "Factory",
            returnType: typeof(Entity),
            parameterTypes: [typeof(ECS)],
            m: typeof(FactoryGenerator).Module
        );
        var generator = dynamicMethod.GetILGenerator();
        
        //Entity entity = world.CreateEntityWithComponents(FactoryGenerator.GetTypes(name);
        var entityLocal = generator.DeclareLocal(typeof(Entity));
        generator.Emit(OpCodes.Ldarg_0);
        //Create type array to insert directly into archetype
        generator.Emit(OpCodes.Ldc_I4, components.Count);
        generator.Emit(OpCodes.Newarr, typeof(Type));
        for (int i = 0; i < components.Count; i++)
        {
            generator.Emit(OpCodes.Dup);
            generator.Emit(OpCodes.Ldc_I4, i);
            generator.Emit(OpCodes.Ldtoken, components[i].Type);
            generator.Emit(OpCodes.Call, TypeFromHandleMethodInfo);
            generator.Emit(OpCodes.Stelem_Ref);
        }
        generator.Emit(OpCodes.Call, CreateEntityMethodInfo);
        generator.Emit(OpCodes.Stloc, entityLocal);
        
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
         
            //ecs.AddComponent(entity, component);
            var setComponentMethodInfo = SetComponentGenericMethodInfo.MakeGenericMethod(componentType);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldloc, entityLocal);
            generator.Emit(OpCodes.Ldloc, compEmitter.ComponentLocal!);
            generator.Emit(OpCodes.Call, setComponentMethodInfo);
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
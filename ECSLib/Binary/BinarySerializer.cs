using System.Runtime.InteropServices;
using ECSLib.Components.Interning;

namespace ECSLib.Binary;

public static class BinarySerializer
{
    public static void Write(ECS world, Stream stream)
    {
        BinaryWriter writer = new(stream);

        var info = world.GetAllInfo().ToList();
        //Entity count
        writer.Write(info.Count);
        //All Entities
        foreach (var (entity, archetype, indexInArchetype) in info)
        {
            //Component Count
            writer.Write(archetype.Count);
            foreach (var (type, bytes) in archetype)
            {
                if (InternedComponentTypesCache.HasInternedField(type))
                    throw new NotImplementedException();

                var componentName = type.AssemblyQualifiedName;
                if (componentName == null) throw new NullReferenceException();
                
                //Component Name
                writer.Write(componentName);
                //Component Data
                writer.Write(bytes);
            }
        }
        
        writer.Close();
    }

    public static void Read(ECS world, Stream stream)
    {
        BinaryReader reader = new(stream); 
        
        //Entity count
        int entityCount = reader.ReadInt32();
        //All Entities
        for (int i = 0; i < entityCount; i++)
        {
            //Component Count
            int componentCount = reader.ReadInt32();
            BinaryComponent[] components = new BinaryComponent[componentCount];
            for (int j = 0; j < componentCount; j++)
            {
                //Component Name
                var componentName = reader.ReadString();
                var componentType = Type.GetType(componentName);
                if (componentType == null)
                    throw new($"Missing component type of name {componentName}");

                if (InternedComponentTypesCache.HasInternedField(componentType))
                    throw new NotImplementedException();
                
                int componentLength = Marshal.SizeOf(componentType);
                //Component Data
                var componentData = reader.ReadBytes(componentLength);
                components[j] = new(componentType, componentData);
            }

            var entity = world.CreateEntityWithComponents(components.Select(comp => comp.Type));
            foreach (var component in components)
                world.SetData(entity, component.Type, component.Bytes);
        }
        
        reader.Close();
    }
}
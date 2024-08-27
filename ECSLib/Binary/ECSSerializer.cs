using ECSLib.Components.Interning;

namespace ECSLib.Binary;

public static class ECSSerializer
{
    public static void WriteWorldToBytes(ECS world, BinaryWriter writer)
    {
        var info = world.GetAllInfo().ToList();
        //Entity count
        writer.Write(info.Count);
        //All Entities
        foreach (var (entity, components) in info)
        {
            var componentList = components.ToList();

            //Component Count
            writer.Write(componentList.Count);
            foreach (var component in componentList)
            {
                var type = component.Type;
                var bytes = component.Bytes.ToArray();
                
                var componentName = type.AssemblyQualifiedName;
                if (componentName == null) throw new NullReferenceException();
                
                var serializedBytes = Serializer.ComponentBytesToSerializedBytes(type, bytes);
                
                //Component Name
                writer.Write(componentName);
                
                //Component Size
                writer.Write(serializedBytes.Length);
                
                //Component Data
                writer.Write(serializedBytes);
            }
        }
    }

    public static void ReadWorldFromBytes(ECS world, BinaryReader reader)
    {
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

                //Component Size
                int componentLength = reader.ReadInt32();
                
                //Component Data
                var componentData = reader.ReadBytes(componentLength);
                components[j] = new(componentType, componentData);
            }

            var entity = world.CreateEntityWithComponents(components.Select(comp => comp.Type));
            foreach (var component in components)
            {
                if (RefPoolContext.CurrentContext == null && InternedComponentTypesCache.HasInternedField(component.Type))
                    RefPoolContext.BeginContext(entity, world);

                var serializedBytes = component.Bytes.ToArray();
                var componentBytes = Serializer.SerializedBytesToComponentBytes(component.Type, serializedBytes);
                world.SetData(entity, component.Type, componentBytes);
            }

            if (RefPoolContext.CurrentContext != null)
                RefPoolContext.EndContext(entity, world);
        }
    }
}
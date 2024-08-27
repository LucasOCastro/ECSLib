using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using ECSLib.Components.Interning;
using ECSLib.Extensions;

namespace ECSLib.Binary;

internal static class Serializer
{
    private static readonly JsonSerializerOptions JsonOptions = new() { IncludeFields = true };
    
    private static ArraySegment<byte> Read(byte[] bytes, int size, ref int index)
    {
        var segment = new ArraySegment<byte>(bytes, index, size);
        index += size;
        return segment;
    }

    private static int ReadInt(byte[] bytes, ref int index) =>
        BitConverter.ToInt32(Read(bytes, sizeof(int), ref index));
    
    public static byte[] ComponentBytesToSerializedBytes(Type type, byte[] componentBytes)
    {
        //TODO consider when fields are reordered - serialize by field name
        //TODO then consider when fields are renamed - serialize by old names too
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        List<byte> result = [];
        int index = 0;
        foreach (var field in fields)
        {
            Type fieldType = field.FieldType;
            if (!fieldType.GenericDefinitionEquals(typeof(PooledRef<>)))
            {
                result.AddRange(Read(componentBytes, Marshal.SizeOf(fieldType), ref index));
                continue;
            }
            
            //If is pooled, retrieve the object itself and write its length + data into the serialized bytes
            Type containedType = fieldType.GenericTypeArguments[0];
            int id = ReadInt(componentBytes, ref index);
            var getMethod = typeof(RefPool<>).MakeGenericType(containedType).GetMethod(
                nameof(RefPool<object>.Get),
                BindingFlags.Static | BindingFlags.Public, 
                [typeof(int)])!;
            var value = getMethod.Invoke(null, [id]);
            var dynamicBytes = JsonSerializer.SerializeToUtf8Bytes(value, containedType, JsonOptions);
            //Write the dynamic size before the bytes
            result.AddRange(BitConverter.GetBytes(dynamicBytes.Length));
            result.AddRange(dynamicBytes);
        }

        return result.ToArray();
    }
        
    public static byte[] SerializedBytesToComponentBytes(Type type, byte[] serializedBytes)
    {
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        List<byte> result = [];
        
        int index = 0;
        foreach (var field in fields)
        {
            Type fieldType = field.FieldType;

            if (!fieldType.GenericDefinitionEquals(typeof(PooledRef<>)))
            {
                result.AddRange(Read(serializedBytes, Marshal.SizeOf(fieldType), ref index));
                continue;
            }
            
            //If is Pooled, deserialize the object and Register, then write the ID to the component bytes
            int readSize = ReadInt(serializedBytes, ref index);
            var readBytes = Read(serializedBytes, readSize, ref index);
            Type containedType = fieldType.GenericTypeArguments[0];
            var pooledObject = JsonSerializer.Deserialize(readBytes, containedType, JsonOptions);
            
            var registerMethod = typeof(RefPool<>).MakeGenericType(containedType).GetMethod(
                nameof(RefPool<object>.Register),
                BindingFlags.Static | BindingFlags.Public,
                [containedType])!;
            int id = (int)registerMethod.Invoke(null, [pooledObject])!;
            result.AddRange(BitConverter.GetBytes(id));
        }

        return result.ToArray();
    }
}
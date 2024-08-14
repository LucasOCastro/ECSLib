namespace ECSLib.Binary;

internal class BinaryComponent
{
    public readonly Type Type;
    public readonly byte[] Bytes;
    
    public BinaryComponent(Type type, byte[] bytes)
    {
        Type = type;
        Bytes = bytes;
    }
}
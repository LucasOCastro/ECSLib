namespace ECSLib.Binary;

internal class BinaryComponent
{
    public readonly Type Type;
    public readonly IReadOnlyCollection<byte> Bytes;
    
    public BinaryComponent(Type type, IReadOnlyCollection<byte> bytes)
    {
        Type = type;
        Bytes = bytes;
    }
}
using System.Runtime.InteropServices;

namespace ECSLib.Components;

/// <summary>
/// Stores components in contiguous memory.
/// </summary>
internal class ComponentCollection
{
    private byte[] _bytes;
    private readonly int _typeSize;
    
    private int CompIndexToByteIndex(int compIndex) => compIndex * _typeSize;

    public Span<byte> GetByteSpanAt(int byteIndex) => new(_bytes, byteIndex, _typeSize);
        
    public Span<TComponent> GetSpanAt<TComponent>(int compIndex) where TComponent : struct =>
        MemoryMarshal.Cast<byte, TComponent>(GetByteSpanAt(CompIndexToByteIndex(compIndex)));

    public void Resize(int extraCount) => 
        Array.Resize(ref _bytes, _bytes.Length + CompIndexToByteIndex(extraCount));
        
    public ComponentCollection(Type type, int count)
    {
        _typeSize = Marshal.SizeOf(type);
        _bytes = new byte[CompIndexToByteIndex(count)];
    }
}
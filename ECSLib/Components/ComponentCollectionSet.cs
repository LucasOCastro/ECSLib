using System.Runtime.InteropServices;

namespace ECSLib.Components;

/// <summary>
/// Stores and manages multiple component arrays.
/// </summary>
internal class ComponentCollectionSet
{
    private const int ComponentCountIncrement = 100;

    private readonly Dictionary<Type, byte[]> _typeToComponents;
    private int _maxCount;
    public int Count { get; private set; }
    private bool IsFull => Count == _maxCount; 
    
    public ComponentCollectionSet(IEnumerable<Type> types)
    {
        _typeToComponents = types.ToDictionary(t => t, t => new byte[ComponentCountIncrement * Marshal.SizeOf(t)]);
        _maxCount = ComponentCountIncrement;
    }

    /// <returns>
    /// The span of the entire component array for a certain component type, or empty if the component is missing.
    /// </returns>
    public Span<T> GetFullSpan<T>() where T : struct =>
        _typeToComponents.TryGetValue(typeof(T), out var byteArray)
            ? MemoryMarshal.Cast<byte, T>(byteArray.AsSpan())
            : Span<T>.Empty;
    
    /// <returns>
    /// A reference to the component.
    /// Do not store this reference, or it will be outdated when the collection updates.
    /// </returns>
    public ref TComponent Get<TComponent>(int index) where TComponent : struct
    {
        var array = _typeToComponents[typeof(TComponent)];
        return ref MemoryMarshal.Cast<byte, TComponent>(array)[index];
    }
    
    /// <summary>
    /// Allocates space for a new component to be filled.
    /// </summary>
    /// <returns>The index of the allocated space for the component.</returns>
    public int RegisterNew()
    {
        if (IsFull) Expand();
        Count++;
        return Count - 1;
    }

    /// <summary> Removes the component at the index and fills up the empty spot. </summary>
    /// <returns> The index of the item which was moved to fill '<see cref="index"/>', or -1 if nothing was moved.</returns>
    public int FreePosition(int index)
    {
        //Fill the position at index and clear the last in the array
        int lastCompIndex = Count - 1;
        foreach (var pair in _typeToComponents)
        {
            int size = Marshal.SizeOf(pair.Key);
            var fullSpan = pair.Value.AsSpan();
            var lastSpan = fullSpan.Slice(lastCompIndex * size, size);
            // If the empty position is at the end of the array, we do not need to fill it up, just clear at the end.
            if (index != lastCompIndex)
            {
                var indexSpan = fullSpan.Slice(index * size, size);
                lastSpan.CopyTo(indexSpan);
            }
            lastSpan.Clear();
        }
        Count--;
        return index != lastCompIndex ? lastCompIndex : -1;
    }
    
    /// <summary>
    /// Expands the component array by <see cref="ComponentCountIncrement"/>.
    /// </summary>
    private void Expand()
    {
        foreach (var pair in _typeToComponents)
        {
            int size = Marshal.SizeOf(pair.Key);
            var array = pair.Value;
            Array.Resize(ref array, array.Length + ComponentCountIncrement * size);
            _typeToComponents[pair.Key] = array;
        }
        _maxCount += ComponentCountIncrement;
    }

    /// <summary>
    /// Copies all the components at position <see cref="fromIndex"/> to the component set <see cref="toSet"/>, where
    /// they will occupy the position <see cref="toIndex"/>.
    /// </summary>
    public void CopyTo(int fromIndex, ComponentCollectionSet toSet, int toIndex)
    {
        foreach (var pair in _typeToComponents)
        {
            if (toSet._typeToComponents.TryGetValue(pair.Key, out var otherArray))
            {
                int size = Marshal.SizeOf(pair.Key);
                var otherSpan = otherArray.AsSpan(toIndex * size, size);
                var thisSpan = pair.Value.AsSpan(fromIndex * size, size);
                thisSpan.CopyTo(otherSpan);
            }
        }
    }
}
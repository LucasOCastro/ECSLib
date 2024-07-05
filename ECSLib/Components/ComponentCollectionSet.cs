namespace ECSLib.Components;

/// <summary>
/// Stores and manages multiple component arrays.
/// </summary>
internal class ComponentCollectionSet
{
    private const int ComponentCountIncrement = 100;

    private readonly Dictionary<Type, ComponentCollection> _typeToComponents;
    private int _maxCount = ComponentCountIncrement;
    private int _count;
    
    private bool IsFull => _count == _maxCount;
    
    public ComponentCollectionSet(IEnumerable<Type> types)
    {
        _typeToComponents = types.ToDictionary(t => t, t => new ComponentCollection(t, ComponentCountIncrement));
    }
    
    /// <returns>
    /// A reference to the component.
    /// Do not store this reference, or it will be outdated when the collection updates.
    /// </returns>
    public ref TComponent Get<TComponent>(int index) where TComponent : struct
    {
        return ref _typeToComponents[typeof(TComponent)].GetSpanAt<TComponent>(index)[0];
    }
    
    /// <summary>
    /// Allocates space for a new component to be filled.
    /// </summary>
    /// <returns>The index of the allocated space for the component.</returns>
    public int RegisterNew()
    {
        if (IsFull)
        {
            Expand();
        }
        _count++;
        return _count - 1;
    }

    /// <summary> Removes the component at the index and fills up the empty spot. </summary>
    /// <returns> The index of the item which was moved to fill '<see cref="index"/>', or -1 if nothing was moved.</returns>
    public int FreePosition(int index)
    {
        //Fill the position at index and clear the last in the array
        int lastCompIndex = _count - 1;
        foreach (var componentArray in _typeToComponents.Values)
        {
            // If the empty position is at the end of the array, we do not need to fill it up, just clear at the end.
            if (index != lastCompIndex)
            {
                var lastSpan = componentArray.GetByteSpanAt(lastCompIndex);
                lastSpan.CopyTo(componentArray.GetByteSpanAt(index));
                lastSpan.Clear();
            }
            componentArray.GetByteSpanAt(lastCompIndex).Clear();
        }
        _count--;
        return index != lastCompIndex ?  lastCompIndex : -1;
    }


    /// <summary>
    /// Expands the component array by <see cref="ComponentCountIncrement"/>.
    /// </summary>
    private void Expand()
    {
        foreach (var componentArray in _typeToComponents.Values)
        {
            componentArray.Resize(ComponentCountIncrement);
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
                var otherSpan = otherArray.GetByteSpanAt(toIndex);
                var thisSpan = pair.Value.GetByteSpanAt(fromIndex);
                thisSpan.CopyTo(otherSpan);
            }
        }
    }
}
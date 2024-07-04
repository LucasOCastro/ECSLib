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
    public void FreePosition(int index)
    {
        FillPosition(index);
        _count--;
    }

    /// <summary>
    /// Moves an element from the end of the component array to the empty index, ensuring it is packed.
    /// </summary>
    private void FillPosition(int emptyCompIndex)
    {
        foreach (var componentArray in _typeToComponents.Values)
        {
            // If the empty position is at the end of the array, we do not need to fill it up, just clear that specific position.
            int lastCompIndex = _count - 1;
            if (emptyCompIndex == lastCompIndex)
            {
                componentArray.GetByteSpanAt(emptyCompIndex).Clear();
                return;
            }

            // Actually perform the update and clear the position which was now left empty.
            var lastSpan = componentArray.GetByteSpanAt(lastCompIndex);
            lastSpan.CopyTo(componentArray.GetByteSpanAt(emptyCompIndex));
            lastSpan.Clear();
        }
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
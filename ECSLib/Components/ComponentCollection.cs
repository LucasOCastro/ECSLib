using System.Runtime.InteropServices;
using ECSLib.Exceptions;

namespace ECSLib.Components;


internal class ComponentCollection
{
    private const int ArrayLengthIncrement = 100;
    
    public int Count { get; private set; }
    
    private byte[] _components = new byte[ArrayLengthIncrement];
    private readonly Dictionary<int, int> _entityIdToCompIndex = new();
    private readonly Dictionary<int, int> _compIndexToEntityId = new();

    private readonly Type _componentType;
    private readonly int _componentSize;
    public ComponentCollection(Type componentType)
    {
        _componentType = componentType;
        _componentSize = Marshal.SizeOf(_componentType);
    }

    private Span<TComponent> GetSpanAt<TComponent>(int index) where TComponent : struct
    {
        Span<byte> span = new(_components, index, _componentSize);
        return MemoryMarshal.Cast<byte, TComponent>(span);
    }
    
    /// <returns>
    /// A reference to the component.
    /// Do not store this reference, or it will be outdated when the collection updates.
    /// </returns>
    /// <exception cref="MissingComponentException">Thrown if the entity does not have the component.</exception>
    public ref TComponent Get<TComponent>(Entity entity) where TComponent : struct
    {
        if (!_entityIdToCompIndex.TryGetValue(entity.ID, out int index))
        {
            throw new MissingComponentException(_componentType, entity);
        }
        return ref GetSpanAt<TComponent>(index)[0];
    }

    /// <summary>
    /// Registers a component associated with the entity.
    /// </summary>
    /// <exception cref="DuplicatedComponentException">Thrown if the entity already has the component.</exception>
    public void Register<TComponent>(Entity entity, TComponent component) where TComponent : struct
    {
        if (_entityIdToCompIndex.ContainsKey(entity.ID))
        {
            throw new DuplicatedComponentException(_componentType, entity);
        }

        if (Count == _components.Length)
        {
            Expand();
        }

        // Adds the component after the last component in the array.
        _entityIdToCompIndex.Add(entity.ID, Count);
        _compIndexToEntityId.Add(Count, entity.ID);
        GetSpanAt<TComponent>(Count).Fill(component);
        Count++;
    }

    /// <summary>
    /// Removes the component associated with the entity..
    /// </summary>
    /// <exception cref="MissingComponentException">Thrown if the entity does not have the component.</exception>
    public void Unregister<TComponent>(Entity entity) where TComponent : struct
    {
        if (!_entityIdToCompIndex.TryGetValue(entity.ID, out int compIndex))
        {
            throw new MissingComponentException(typeof(TComponent), entity);
        }
        
        _entityIdToCompIndex.Remove(entity.ID);
        _compIndexToEntityId.Remove(compIndex);
        FillPosition<TComponent>(compIndex);
        Count--;
    }

    /// <summary>
    /// Moves an element from the end of the component array to the empty index, ensuring it is packed.
    /// </summary>
    private void FillPosition<TComponent>(int emptyCompIndex) where TComponent : struct
    {
        // If the empty position is at the end of the array, we do not need to fill it up, just clear that specific position.
        int lastCompIndex = Count - 1;
        if (emptyCompIndex == lastCompIndex)
        {
            GetSpanAt<TComponent>(emptyCompIndex).Clear();
            return;
        }
        
        // There is no entity associated with the last index anymore, so remove it.
        _compIndexToEntityId.Remove(lastCompIndex);
        
        // The position of the last entity's component was changed to the emptyCompIndex, so update it.
        int lastEntityId = _compIndexToEntityId[Count - 1];
        _entityIdToCompIndex[lastEntityId] = emptyCompIndex;

        // Actually perform the update and clear the position which was now left empty.
        GetSpanAt<TComponent>(lastCompIndex).CopyTo(GetSpanAt<TComponent>(emptyCompIndex));
        GetSpanAt<TComponent>(lastCompIndex).Clear();
    }

    /// <summary>
    /// Expands the component array by <see cref="ArrayLengthIncrement"/>.
    /// </summary>
    private void Expand()
    {
        Array.Resize(ref _components, _componentSize * (Count + ArrayLengthIncrement));
    }
}
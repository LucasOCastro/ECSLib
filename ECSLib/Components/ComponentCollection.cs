using System.Runtime.InteropServices;
using ECSLib.Components.Exceptions;
using ECSLib.Entities;

namespace ECSLib.Components;


internal class ComponentCollection
{
    private const int ComponentCountIncrement = 100;
    
    public int Count { get; private set; }
    
    private byte[] _components;
    private readonly Dictionary<Entity, int> _entityToCompIndex = new();
    private readonly Dictionary<int, Entity> _compIndexToEntity = new();

    private readonly Type _componentType;
    private readonly int _componentSize;
    public ComponentCollection(Type componentType)
    {
        _componentType = componentType;
        _componentSize = Marshal.SizeOf(_componentType);
        _components = new byte[CompIndexToByteIndex(ComponentCountIncrement)];
    }

    private int CompIndexToByteIndex(int compIndex) => compIndex * _componentSize;
    
    private Span<byte> GetByteSpanAt(int byteIndex) => new(_components, byteIndex, _componentSize);

    private Span<TComponent> GetSpanAt<TComponent>(int compIndex) where TComponent : struct =>
        MemoryMarshal.Cast<byte, TComponent>(GetByteSpanAt(CompIndexToByteIndex(compIndex)));

    private bool IsFull() => CompIndexToByteIndex(Count) == _components.Length;

    /// <returns>
    /// A reference to the component.
    /// Do not store this reference, or it will be outdated when the collection updates.
    /// </returns>
    /// <exception cref="MissingComponentException">Thrown if the entity does not have the component.</exception>
    public ref TComponent Get<TComponent>(Entity entity) where TComponent : struct
    {
        if (!_entityToCompIndex.TryGetValue(entity, out int index))
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
        if (_entityToCompIndex.ContainsKey(entity))
        {
            throw new DuplicatedComponentException(_componentType, entity);
        }

        if (IsFull())
        {
            Expand();
        }

        // Adds the component after the last component in the array.
        _entityToCompIndex.Add(entity, Count);
        _compIndexToEntity.Add(Count, entity);
        GetSpanAt<TComponent>(Count).Fill(component);
        Count++;
    }

    /// <summary>
    /// Removes the component associated with the entity..
    /// </summary>
    /// <exception cref="MissingComponentException">Thrown if the entity does not have the component.</exception>
    public void Unregister(Entity entity)
    {
        if (!_entityToCompIndex.TryGetValue(entity, out int compIndex))
        {
            throw new MissingComponentException(_componentType, entity);
        }
        
        _entityToCompIndex.Remove(entity);
        _compIndexToEntity.Remove(compIndex);
        FillPosition(compIndex);
        Count--;
    }

    /// <summary>
    /// Moves an element from the end of the component array to the empty index, ensuring it is packed.
    /// </summary>
    private void FillPosition(int emptyCompIndex)
    {
        // If the empty position is at the end of the array, we do not need to fill it up, just clear that specific position.
        int lastCompIndex = Count - 1;
        if (emptyCompIndex == lastCompIndex)
        {
            GetByteSpanAt(emptyCompIndex).Clear();
            return;
        }
        
        // There is no entity associated with the last index anymore, so remove it.
        _compIndexToEntity.Remove(lastCompIndex);
        
        // The position of the last entity's component was changed to the emptyCompIndex, so update it.
        var lastEntity = _compIndexToEntity[Count - 1];
        _entityToCompIndex[lastEntity] = emptyCompIndex;

        // Actually perform the update and clear the position which was now left empty.
        GetByteSpanAt(lastCompIndex).CopyTo(GetByteSpanAt(emptyCompIndex));
        GetByteSpanAt(lastCompIndex).Clear();
    }

    /// <summary>
    /// Expands the component array by <see cref="ArrayLengthIncrement"/>.
    /// </summary>
    private void Expand()
    {
        Array.Resize(ref _components, _components.Length + CompIndexToByteIndex(ComponentCountIncrement));
    }
}
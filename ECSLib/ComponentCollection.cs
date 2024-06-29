using System.Collections;
using ECSLib.Exceptions;

namespace ECSLib;

internal class ComponentCollection<TComponent> : IEnumerable<TComponent> where TComponent : struct
{
    private const int ArrayLengthIncrement = 100;
    
    public int Count { get; private set; }
    
    private TComponent[] _components = new TComponent[ArrayLengthIncrement];
    private readonly Dictionary<int, int> _entityIdToCompIndex = new();
    private readonly Dictionary<int, int> _compIndexToEntityId = new();
    
    /// <returns>
    /// A reference to the component. Do not store this reference, or it will
    /// be outdated when the collection updates.
    /// </returns>
    /// <exception cref="MissingComponentException">Thrown if the entity does not have the component.</exception>
    public ref TComponent Get(Entity entity)
    {
        if (!_entityIdToCompIndex.TryGetValue(entity.ID, out int index))
        {
            throw new MissingComponentException(typeof(TComponent), entity);
        }
        return ref _components[index];
    }

    /// <summary>
    /// Registers a component associated with the entity.
    /// </summary>
    /// <exception cref="DuplicatedComponentException">Thrown if the entity already has the component.</exception>
    public void Register(Entity entity, TComponent component)
    {
        if (_entityIdToCompIndex.ContainsKey(entity.ID))
        {
            throw new DuplicatedComponentException(typeof(TComponent), entity);
        }

        if (Count == _components.Length)
        {
            Expand();
        }

        _entityIdToCompIndex.Add(entity.ID, Count);
        _compIndexToEntityId.Add(Count, entity.ID);
        _components[Count] = component;
        Count++;
    }

    /// <summary>
    /// Removes the component associated with the entity..
    /// </summary>
    /// <exception cref="MissingComponentException">Thrown if the entity does not have the component.</exception>
    public void Unregister(Entity entity)
    {
        if (!_entityIdToCompIndex.TryGetValue(entity.ID, out int compIndex))
        {
            throw new MissingComponentException(typeof(TComponent), entity);
        }
        
        _entityIdToCompIndex.Remove(entity.ID);
        _compIndexToEntityId.Remove(compIndex);
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
            _components[emptyCompIndex] = default;
            return;
        }
        
        // There is no entity associated with the last index anymore, so remove it.
        _compIndexToEntityId.Remove(lastCompIndex);
        
        // The position of the last entity's component was changed to the emptyCompIndex, so update it.
        int lastEntityId = _compIndexToEntityId[Count - 1];
        _entityIdToCompIndex[lastEntityId] = emptyCompIndex;

        // Actually perform the update and clear the posiiton which was now left empty.
        _components[emptyCompIndex] = _components[lastCompIndex];
        _components[lastCompIndex] = default;
    }

    /// <summary>
    /// Expands the component array by <see cref="ArrayLengthIncrement"/>.
    /// </summary>
    private void Expand()
    {
        Array.Resize(ref _components, Count + ArrayLengthIncrement);
    }

    public IEnumerator<TComponent> GetEnumerator() => _components.AsEnumerable().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
using ECSLib.Entities;

namespace ECSLib.Components.Exceptions;

public class MissingComponentException : Exception
{
    public MissingComponentException(Type componentType, Entity entity)
        : base($"Component {componentType.Name} not present in entity {entity.ID}.")
    {
    }
}
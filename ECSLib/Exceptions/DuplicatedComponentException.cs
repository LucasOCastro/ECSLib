namespace ECSLib.Exceptions;

public class DuplicatedComponentException : Exception
{
    public DuplicatedComponentException(Type componentType, Entity entity)
        : base($"Component {componentType.Name} already present in entity {entity.ID}.")
    {
    }
}
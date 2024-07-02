using ECSLib.Entities;

namespace ECSLib.Components.Exceptions;

public class MissingComponentException(Type componentType, Entity entity)
    : Exception($"Component {componentType.Name} not present in entity {entity.ID}.");
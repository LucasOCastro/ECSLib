using ECSLib.Entities;

namespace ECSLib.Components.Exceptions;

public class DuplicatedComponentException(Type componentType, Entity entity)
    : Exception($"Component {componentType.Name} already present in entity {entity.ID}.");
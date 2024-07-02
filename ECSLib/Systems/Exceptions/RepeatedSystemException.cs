namespace ECSLib.Systems.Exceptions;

internal class RepeatedSystemException(Type getType)
    : Exception($"A system of type {getType.Name} was already registered.");
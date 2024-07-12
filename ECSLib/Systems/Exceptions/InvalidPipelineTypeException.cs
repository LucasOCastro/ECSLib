namespace ECSLib.Systems.Exceptions;

internal class InvalidPipelineTypeException(Type pipelineEnumType)
    : Exception($"{pipelineEnumType.FullName} is not a valid enum type.");
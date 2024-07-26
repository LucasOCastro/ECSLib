namespace ECSLib.Systems.Exceptions;

internal class InvalidPipelineValueException(Type pipelineEnumType, int pipeline)
    : Exception($"{pipeline} is not a valid numerical value for an enum of type {pipelineEnumType.FullName}.");
using ECSLib.Systems.Attributes;

namespace ECSLib.Systems.Exceptions;

internal class MissingPipelineEnumTypeException()
    : Exception($"Tried to register a pipeline enum type but no enum was flagged with {nameof(PipelineEnumAttribute)}.");
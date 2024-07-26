namespace ECSLib.Systems.Attributes;

[AttributeUsage(AttributeTargets.Enum)]
public class PipelineEnumAttribute : Attribute;

[AttributeUsage(AttributeTargets.Field)]
public class DoNotProcessAttribute : Attribute;
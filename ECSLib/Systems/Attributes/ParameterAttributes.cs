namespace ECSLib.Systems.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class AnyAttribute : Attribute;

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class OptionalAttribute : Attribute;
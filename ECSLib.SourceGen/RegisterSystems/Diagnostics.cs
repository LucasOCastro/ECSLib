using Microsoft.CodeAnalysis;

namespace ECSLib.SourceGen.RegisterSystems;

public static class Diagnostics
{
    public static readonly DiagnosticDescriptor SystemClassPartial = new(
        "ECS0001",
        "Invalid system class (partial)",
        "Class with ECSSystem attribute must be partial",
        "Usage", DiagnosticSeverity.Error, true);
    
    public static readonly DiagnosticDescriptor SystemClassGeneric = new(
        "ECS0002",
        "Invalid system class (non-generic)",
        "Class with ECSSystem attribute must not be generic",
        "Usage", DiagnosticSeverity.Error, true);
    
    public static readonly DiagnosticDescriptor SystemClassInherit = new(
        "ECS0003",
        "Invalid system class (inherit)",
        $"Class with ECSSystem attribute must inherit ECSLib.Systems.BaseSystem",
        "Usage", DiagnosticSeverity.Error, true);
    
    public static readonly DiagnosticDescriptor NestedClass = new(
        "ECS0004",
        "Invalid system class (non-nested)",
        "Class with ECSSystem attribute must not be nested",
        "Usage", DiagnosticSeverity.Error, true);
    
    public static readonly DiagnosticDescriptor EntityParamOrder = new(
        "ECS0005",
        "Invalid parameter order",
        "The entity parameter in an ECS system should come first",
        "Usage", DiagnosticSeverity.Error, true);
    
    public static readonly DiagnosticDescriptor ParamRefStruct = new(
        "ECS0006",
        "Invalid system parameter",
        "A component parameter in an ECS system must be a struct passed by reference",
        "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor RequiredParam = new(
        "ECS0007",
        "Empty system query",
        "A system method requires at least one required param for a query",
        "Usage", DiagnosticSeverity.Error, true);

    public static readonly DiagnosticDescriptor OptionalParamWrap = new(
        "ECS0008",
        "Unwrapped optional parameter",
        "An optional system parameter must be wrapped in Comp<T>",
        "Usage", DiagnosticSeverity.Error, true);
}
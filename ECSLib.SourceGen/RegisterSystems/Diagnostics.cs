using Microsoft.CodeAnalysis;

namespace ECSLib.SourceGen.RegisterSystems;

public static class Diagnostics
{
    public static DiagnosticDescriptor SystemClassPartialError(string attributeName) => new(
        "ECSG001",
        "Invalid system class",
        $"Class with {attributeName} attribute must be partial.",
        "Problem", DiagnosticSeverity.Error, true);
    
    public static DiagnosticDescriptor SystemClassGenericError(string attributeName) => new(
        "ECSG001",
        "Invalid system class",
        $"Class with {attributeName} attribute must not be generic.",
        "Problem", DiagnosticSeverity.Error, true);
    
    public static DiagnosticDescriptor SystemClassInheritError(string attributeName, string baseClassName) => new(
        "ECSG001",
        "Invalid system class",
        $"Class with {attributeName} attribute must inherit {baseClassName}.",
        "Problem", DiagnosticSeverity.Error, true);
    
    public static DiagnosticDescriptor NestedClassError(string attributeName) => new(
        "ECSG001",
        "Invalid system class",
        $"Class with {attributeName} must not be nested.",
        "Problem", DiagnosticSeverity.Error, true);
    
    public static DiagnosticDescriptor EntityParamOrderError() => new(
        "ECSG002",
        "Invalid system parameters",
        "The entity parameter in an ECS system should come first.",
        "Problem", DiagnosticSeverity.Error, true);
    
    public static DiagnosticDescriptor ParamRefStructError() => new(
        "ECSG003",
        "Invalid system parameters",
        "The component parameter in an ECS must be a struct passed by reference.",
        "Problem", DiagnosticSeverity.Error, true);

    public static DiagnosticDescriptor RequiredParamError() => new(
        "ECSG004",
        "Empty system query",
        "A system method requires at least one required param for a query.",
        "Problem", DiagnosticSeverity.Error, true);

    public static DiagnosticDescriptor OptionalParamWrapError() => new(
        "ECSG005",
        "Unwrapped optional parameter",
        "An optional system parameter must be wrapped in Comp<T>.",
        "Problem", DiagnosticSeverity.Error, true);
}
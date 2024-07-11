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
    
    public static DiagnosticDescriptor SystemContainingClassError(string attributeName) => new(
        "ECSG001",
        "Invalid system class",
        $"Class with {attributeName} must not be nested.",
        "Problem", DiagnosticSeverity.Error, true);
}
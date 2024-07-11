using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using CSharpExtensions = Microsoft.CodeAnalysis.CSharpExtensions;

namespace ECSLib.SourceGen.Utilities;

internal static class SymbolExtension
{
    public static bool IsPartial(this ISymbol symbol) => symbol.DeclaringSyntaxReferences.Any(syntax =>
        syntax.GetSyntax() is BaseTypeDeclarationSyntax declaration &&
        declaration.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PartialKeyword)));
    
    /// <returns>
    /// The containing namespace symbols, from closest to furthest.
    /// </returns>
    public static IEnumerable<INamespaceOrTypeSymbol> GetParentNamespaces(this ISymbol symbol)
    {
        var parentNamespace = symbol.ContainingNamespace;
        while (parentNamespace != null && !string.IsNullOrEmpty(parentNamespace.MetadataName))
        {
            yield return parentNamespace;
            parentNamespace = parentNamespace.ContainingNamespace;
        }
    }
}
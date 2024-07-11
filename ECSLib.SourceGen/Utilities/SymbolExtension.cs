using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

    private static bool IsRootNamespace(ISymbol? symbol) => symbol is INamespaceSymbol { IsGlobalNamespace: true }; 
    public static string GetFullMetadataName(this ISymbol? symbol)
    {
        if (symbol == null || IsRootNamespace(symbol))
        {
            return string.Empty;
        }

        StringBuilder sb = new(symbol.MetadataName);
        var lastSymbol = symbol;
        symbol = symbol.ContainingSymbol;
        while (!IsRootNamespace(symbol))
        {
            if (symbol is ITypeSymbol && lastSymbol is ITypeSymbol)
                sb.Insert(0, '+');
            else
                sb.Insert(0, '.');
            sb.Insert(0, symbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat));
            symbol = symbol.ContainingSymbol;
        }

        return sb.ToString();
    }
}
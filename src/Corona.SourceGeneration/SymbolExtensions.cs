using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Corona.SourceGeneration;

internal static class SymbolExtensions
{
    public static bool HasAttribute(this ISymbol symbol, string metadataName) =>
        symbol.GetAttributes().Any(a => a.AttributeClass?.ToDisplayString() == metadataName);
    public static bool IsPartial(this INamedTypeSymbol symbol, CancellationToken token = default) =>
        symbol.DeclaringSyntaxReferences.Length > 0 && symbol.DeclaringSyntaxReferences.All(r =>
            r.GetSyntax(token) is TypeDeclarationSyntax declaration && declaration.Modifiers.Any(SyntaxKind.PartialKeyword));
    public static bool InheritsFrom(this INamedTypeSymbol symbol, string metadataName)
    {
        for (var parent = symbol.BaseType; parent is not null; parent = parent.BaseType)
            if (parent.ToDisplayString() == metadataName) return true;
        return false;
    }
}

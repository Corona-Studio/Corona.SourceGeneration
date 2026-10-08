using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Corona.SourceGeneration;

internal static class CSharpNames
{
    public static string Identifier(string name) =>
        SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None || SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None
            ? "@" + name : name;
    public static bool IsIdentifier(string name) => name.Length > 0 && name[0] != '@' &&
        (SyntaxFacts.IsValidIdentifier(name) || SyntaxFacts.GetKeywordKind(name) != SyntaxKind.None ||
         SyntaxFacts.GetContextualKeywordKind(name) != SyntaxKind.None);
    public static string Literal(string value) => SymbolDisplay.FormatLiteral(value, true);
    public static string Namespace(INamespaceSymbol symbol) => symbol.IsGlobalNamespace ? "" :
        string.Join(".", symbol.ToDisplayString().Split('.').Select(Identifier));
    public static string MetadataName(INamedTypeSymbol type) => type.ContainingType is null
        ? (type.ContainingNamespace.IsGlobalNamespace ? "" : type.ContainingNamespace.ToDisplayString() + ".") + type.MetadataName
        : MetadataName(type.ContainingType) + "+" + type.MetadataName;
    // Escape underscores too, so A.B and A_B can never collide.
    public static string HintName(string identity)
    {
        var result = new StringBuilder();
        foreach (var c in identity)
            if (char.IsLetterOrDigit(c)) result.Append(c);
            else result.Append('_').Append(((int)c).ToString("X4", CultureInfo.InvariantCulture)).Append('_');
        return result.ToString();
    }
    public static string PropertyName(string field)
    {
        var name = field.StartsWith("m_", System.StringComparison.Ordinal) ? field.Substring(2) : field.TrimStart('_');
        return name.Length == 0 ? "" : char.ToUpperInvariant(name[0]) + name.Substring(1);
    }
}

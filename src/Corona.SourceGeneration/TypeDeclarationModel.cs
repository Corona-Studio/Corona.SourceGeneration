using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Corona.SourceGeneration;

/// <summary>Symbol-free partial type hierarchy, including nested/generic types and the global namespace.</summary>
internal sealed record TypeDeclarationModel(string Namespace, string Identity, EquatableArray<string> Declarations)
{
    public static TypeDeclarationModel From(INamedTypeSymbol symbol)
    {
        var declarations = new Stack<string>();
        for (var type = symbol; type is not null; type = type.ContainingType)
        {
            var kind = type.IsRecord ? (type.IsValueType ? "record struct" : "record class") :
                type.TypeKind == TypeKind.Interface ? "interface" : type.IsValueType ? "struct" : "class";
            var modifiers = (type.IsStatic ? "static " : type.IsValueType && type.IsReadOnly ? "readonly " : "") +
                (type.IsRefLikeType ? "ref " : "");
            var parameters = type.TypeParameters.Length == 0 ? "" : "<" +
                string.Join(", ", type.TypeParameters.Select(p =>
                    (p.Variance == VarianceKind.In ? "in " : p.Variance == VarianceKind.Out ? "out " : "") + CSharpNames.Identifier(p.Name))) + ">";
            declarations.Push(modifiers + "partial " + kind + " " + CSharpNames.Identifier(type.Name) + parameters);
        }
        return new(CSharpNames.Namespace(symbol.ContainingNamespace), CSharpNames.MetadataName(symbol), new(declarations));
    }
    public void Open(CodeWriter writer, string baseTypes = "")
    {
        if (Namespace.Length > 0) writer.Open("namespace " + Namespace);
        for (var i = 0; i < Declarations.Count; i++)
            writer.Open(Declarations[i] + (i == Declarations.Count - 1 ? baseTypes : ""));
    }
    public void Close(CodeWriter writer)
    {
        for (var i = 0; i < Declarations.Count; i++) writer.Close();
        if (Namespace.Length > 0) writer.Close();
    }
}

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Corona.SourceGeneration;

/// <summary>Diagnostic data without retaining syntax trees or compilations.</summary>
internal sealed record DiagnosticInfo(DiagnosticDescriptor Descriptor, string Path, TextSpan Span,
    LinePositionSpan Lines, EquatableArray<string> Arguments)
{
    public static DiagnosticInfo Create(DiagnosticDescriptor descriptor, Location? location, params string[] arguments)
    {
        var span = location?.GetLineSpan() ?? default;
        return new(descriptor, span.Path ?? "", location?.SourceSpan ?? default, span.Span, new(arguments));
    }
    public Diagnostic ToDiagnostic() => Diagnostic.Create(Descriptor,
        Path.Length == 0 ? Location.None : Location.Create(Path, Span, Lines), System.Linq.Enumerable.ToArray(Arguments));
}

internal sealed record GenerationResult<T>(T? Value, EquatableArray<DiagnosticInfo> Diagnostics) where T : class;

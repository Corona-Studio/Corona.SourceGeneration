using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Corona.SourceGeneration;

internal sealed record SourceFile(string HintName, string Text);

internal static class SourceOutput
{
    public static void AddSource(this SourceProductionContext context, SourceFile source)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        context.AddSource(source.HintName, SourceText.From(source.Text, Encoding.UTF8));
    }
}

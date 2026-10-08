using Corona.SourceGeneration;
using Microsoft.CodeAnalysis;

namespace PackageConsumer;

[Generator]
public sealed class SmokeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var source = context.CompilationProvider.Select(static (_, _) =>
            new SourceFile("Smoke.g.cs", new CodeWriter().Header().Open("class Smoke").Close().ToString()));
        context.RegisterSourceOutput(source, static (output, model) => output.AddSource(model));
    }
}

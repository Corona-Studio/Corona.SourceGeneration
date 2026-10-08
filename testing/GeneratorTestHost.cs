using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace Corona.SourceGeneration.Testing;

/// <summary>Source-linked test support for net8.0+ test projects. Does not depend on a test framework.</summary>
internal static class GeneratorTestHost
{
    public static readonly CSharpParseOptions ParseOptions = new(LanguageVersion.Preview);
    private static readonly ImmutableArray<MetadataReference> References =
        ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!).Split(Path.PathSeparator)
        .Select(static path => (MetadataReference)MetadataReference.CreateFromFile(path)).ToImmutableArray();

    public static CSharpCompilation Compilation(string source, string name = "GeneratorTests") =>
        CSharpCompilation.Create(name, new[] { CSharpSyntaxTree.ParseText(source, ParseOptions) }, References,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));

    public static GeneratorDriver Driver(IIncrementalGenerator generator, params AdditionalText[] files) =>
        CSharpGeneratorDriver.Create(new[] { generator.AsSourceGenerator() }, files, ParseOptions,
            new TestOptions(), new GeneratorDriverOptions(IncrementalGeneratorOutputKind.None, trackIncrementalGeneratorSteps: true));

    public static GeneratorDriver Run(GeneratorDriver driver, Compilation compilation, bool requireCompilation = true)
    {
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var output, out var diagnostics);
        var errors = diagnostics.Concat(requireCompilation ? output.GetDiagnostics() : Array.Empty<Diagnostic>())
            .Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error).ToArray();
        if (errors.Length > 0) throw new InvalidOperationException(string.Join("\n", errors.Select(static diagnostic => diagnostic.ToString())));
        return driver;
    }

    public static string Text(GeneratorDriver driver) => string.Join("\n", driver.GetRunResult().GeneratedTrees.Select(static tree => tree.ToString()));

    private sealed class TestOptions : AnalyzerConfigOptionsProvider
    {
        public override AnalyzerConfigOptions GlobalOptions { get; } = new Global();
        public override AnalyzerConfigOptions GetOptions(SyntaxTree tree) => GlobalOptions;
        public override AnalyzerConfigOptions GetOptions(AdditionalText textFile) => GlobalOptions;
        private sealed class Global : AnalyzerConfigOptions
        {
            public override bool TryGetValue(string key, out string value)
            {
                value = key.Equals("build_property.ProjectDir", StringComparison.OrdinalIgnoreCase) ? Path.GetTempPath() : "";
                return value.Length > 0;
            }
        }
    }
}

internal sealed class MemoryAdditionalText(string path, string text) : AdditionalText
{
    public override string Path { get; } = path;
    public override SourceText GetText(CancellationToken cancellationToken = default) => SourceText.From(text);
}

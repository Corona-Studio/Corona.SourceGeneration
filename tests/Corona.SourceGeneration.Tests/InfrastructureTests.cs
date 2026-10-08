using System.Collections.Immutable;
using Corona.SourceGeneration.Testing;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Corona.SourceGeneration.Tests;

[TestClass]
public sealed class InfrastructureTests
{
    [TestMethod]
    public void ArraysCompareStructurallyIncludingDefaultEmptyAndNull()
    {
        var left = new EquatableArray<string?>(new[] { "a", null });
        var right = new EquatableArray<string?>(new[] { "a", null });
        Assert.AreEqual(left, right);
        Assert.AreEqual(left.GetHashCode(), right.GetHashCode());
        Assert.AreEqual(default(EquatableArray<int>), new EquatableArray<int>(Array.Empty<int>()));
        Assert.AreNotEqual(new EquatableArray<int>(new[] { 1, 2 }), new EquatableArray<int>(new[] { 2, 1 }));
    }

    [TestMethod]
    public void NamesEscapeLiteralsKeywordsAndCollidingMetadata()
    {
        Assert.AreEqual("@class", CSharpNames.Identifier("class"));
        Assert.AreEqual("Value", CSharpNames.PropertyName("m_value"));
        Assert.AreEqual("", CSharpNames.PropertyName("___"));
        Assert.AreNotEqual(CSharpNames.HintName("A.B"), CSharpNames.HintName("A_B"));
        var literal = CSharpNames.Literal("\"\\\n");
        Assert.AreEqual("\"\\\n", SyntaxFactory.ParseExpression(literal).GetFirstToken().ValueText);
    }

    [TestMethod]
    public void NestedGenericRecordAndGlobalDeclarationsCompile()
    {
        var compilation = GeneratorTestHost.Compilation("""
            namespace @namespace
            {
                public partial class Outer<T> where T : class
                {
                    public readonly partial record struct @event<U>;
                }
            }
            public static partial class Global;
            """);
        var symbol = compilation.GetTypeByMetadataName("namespace.Outer`1+event`1")!;
        var model = TypeDeclarationModel.From(symbol);
        Assert.AreEqual(model, TypeDeclarationModel.From(symbol));
        var writer = new CodeWriter().Header();
        model.Open(writer); writer.Line("public int Generated => 1;"); model.Close(writer);
        var global = TypeDeclarationModel.From(compilation.GetTypeByMetadataName("Global")!);
        global.Open(writer); writer.Line("public static int Value => 2;"); global.Close(writer);
        var output = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(writer.ToString(), GeneratorTestHost.ParseOptions));
        Assert.AreEqual(0, output.GetDiagnostics().Count(static d => d.Severity == DiagnosticSeverity.Error),
            string.Join("\n", output.GetDiagnostics()));
        Assert.IsFalse(writer.ToString().Contains('\r'));
    }

    [TestMethod]
    public void WriterRejectsUnbalancedBlocks()
    {
        Assert.Throws<InvalidOperationException>(() => new CodeWriter().Close());
        Assert.Throws<InvalidOperationException>(() => new CodeWriter().Open("class C").ToString());
    }

    [TestMethod]
    public void UnrelatedEditsReuseOutput()
    {
        var compilation = GeneratorTestHost.Compilation("class Initial { }");
        var driver = GeneratorTestHost.Run(GeneratorTestHost.Driver(new ConstantGenerator()), compilation);
        driver = GeneratorTestHost.Run(driver, compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText("class Unrelated { }", GeneratorTestHost.ParseOptions)));
        Assert.AreEqual(IncrementalStepRunReason.Unchanged,
            driver.GetRunResult().Results.Single().TrackedSteps["Model"].Single().Outputs.Single().Reason);
        Assert.AreEqual(IncrementalStepRunReason.Cached,
            driver.GetRunResult().Results.Single().TrackedOutputSteps.Values.SelectMany(static steps => steps)
                .SelectMany(static step => step.Outputs).Single().Reason);
    }

    private sealed class ConstantGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var models = context.CompilationProvider.Select(static (_, _) =>
                new SourceFile("Constant.g.cs", "class Generated { }")).WithTrackingName("Model");
            context.RegisterSourceOutput(models, static (output, model) => output.AddSource(model));
        }
    }
}

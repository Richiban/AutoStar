using System.Collections.Immutable;
using System.Reflection;

namespace AutoStar.Tests.Common;

public class GeneratorTestRunner
{
    private readonly ISourceGenerator _sourceGenerator;

    public GeneratorTestRunner(ISourceGenerator sourceGenerator)
    {
        _sourceGenerator = sourceGenerator;
    }

    public TestRunResult RunGenerator(string source)
    {
        var inputCompilation = CSharpCompilation.Create(
            "compilation",
            new[] { CSharpSyntaxTree.ParseText(source) },
            new[]
            {
                MetadataReference.CreateFromFile(
                    typeof(Binder).GetTypeInfo().Assembly.Location)
            },
            new CSharpCompilationOptions(OutputKind.ConsoleApplication));

        var driver = CSharpGeneratorDriver.Create(_sourceGenerator)
            .RunGeneratorsAndUpdateCompilation(
                inputCompilation,
                out var outputCompilation,
                out var diagnostics);

        return new(outputCompilation, diagnostics);
    }
}

public class TestRunResult
{
    public TestRunResult(
        Compilation outputCompilation,
        ImmutableArray<Diagnostic> diagnostics)
    {
        OutputCompilation = outputCompilation;
        Diagnostics = diagnostics;
    }

    public ImmutableArray<Diagnostic> Diagnostics { get; }
    public Compilation OutputCompilation { get; }

    public void Deconstruct(
        out Compilation outputCompilation,
        out ImmutableArray<Diagnostic> diagnostics)
    {
        outputCompilation = OutputCompilation;
        diagnostics = Diagnostics;
    }

    public SyntaxTree GetSourceFile(string fileName)
    {
        if (OutputCompilation.SyntaxTrees.SingleOrDefault(
                s => s.FilePath.EndsWith(fileName)) is { } syntaxTree)
        {
            return syntaxTree;
        }

        var fileNamesString = string.Join(
            ", ",
            OutputCompilation.SyntaxTrees.Select(s => $"'{s.FilePath}'"));

        throw new InvalidOperationException(
            $"Could not find an output file with the name {fileName}. Files: {fileNamesString}");
    }
}
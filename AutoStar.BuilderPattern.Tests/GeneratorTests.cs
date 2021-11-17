using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Shouldly;

namespace AutoStar.BuilderPattern.Tests
{
    [TestFixture]
    internal class GeneratorTests
    {
        [Test]
        public void InjectedAttributeFileTest()
        {
            var source = @"
[BuilderPattern]
public partial class TestClass
{
}
";

            var (outputCompilation, diagnostics) = RunGenerator(source);

            diagnostics.ShouldBeEmpty();

            var src = GetSourceFile(outputCompilation, "BuilderPatternAttribute.g.cs")
                .GetText()
                .ToString();

            src.ShouldBe(
                @"using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class BuilderPatternAttribute : Attribute
{}");
        }

        [Test]
        public void NonPartialClassGivesDiagnosticError()
        {
            var source = @"
using System;

namespace TestSamples
{
    [BuilderPattern]
    public class TestClass
    {
    }
}
";

            var (_, diagnostics) = RunGenerator(source);

            var diagnostic = diagnostics.ShouldHaveSingleItem();
            diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
            diagnostic.Id.ShouldBe("ASBP0001");

            diagnostic.GetMessage()
                .ShouldBe(
                    "The class TestClass must be partial in order to use the BuilderPattern attribute");
        }

        [Test]
        public void BaseCase()
        {
            var source = @"
namespace TestSamples
{
    [BuilderPattern]
    public partial class TestClass
    {
    }
}
";

            var (compilation, diagnostics) = RunGenerator(source);

            Assert.That(diagnostics, Is.Empty);

            var programText = GetSourceFile(compilation, "TestClass.g.cs")
                .GetText()
                .ToString();

            programText.ShouldBe(
                @"namespace TestSamples
{
    public partial class TestClass
    {
        public class Builder
        {
        }
    }
}");
        }

        private static SyntaxTree GetSourceFile(
            Compilation outputCompilation,
            string fileName)
        {
            switch (outputCompilation.SyntaxTrees.SingleOrDefault(
                s => s.FilePath.EndsWith(fileName)))
            {
                case { } a:
                    return a;
                default:

                    var fileNamesString = string.Join(
                        ", ",
                        outputCompilation.SyntaxTrees.Select(s => $"'{s.FilePath}'"));

                    throw new InvalidOperationException(
                        $"Could not find an output file with the name {fileName}. Files: {fileNamesString}");
            }
        }

        private static (Compilation, ImmutableArray<Diagnostic>) RunGenerator(
            string source)
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

            var generator = new BuilderPatternGenerator();

            var driver = CSharpGeneratorDriver.Create(generator)
                .RunGeneratorsAndUpdateCompilation(
                    inputCompilation,
                    out var outputCompilation,
                    out var diagnostics);

            return (outputCompilation, diagnostics);
        }
    }
}
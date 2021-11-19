using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Linq;
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
            var source = @"";

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
        public void MultipleConstructorsGivesDiagnosticError()
        {
            var source = @"
namespace TestSamples
{
    [BuilderPattern]
    public partial class TestClass
    {
        public TestClass() {}
        public TestClass(string a) {}
    }
}
";

            var (compilation, diagnostics) = RunGenerator(source);

            var diagnostic = diagnostics.ShouldHaveSingleItem();

            diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);

            diagnostic.GetMessage()
                .ShouldBe(
                    "Class TestClass must define exactly one constructor to use the BuilderPattern attribute");
        }

        [Test]
        public void PrimaryConstructorAlsoGenerated()
        {
            var source = @"
namespace TestSamples
{
    [BuilderPattern]
    public partial class TestClass
    {
        public string A { get; }
        public string B { get; }
        public int    C { get; }
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
            public string A { get; }
            public string B { get; }
            public int    C { get; }

            public TestClass Build()
            {
                var validationErrors = new List<string>();

                if (A == null)
                {
                    validationErrors.Add(""A is required"");
                }

                if (B == null)
                {
                    validationErrors.Add(""B is required"");
                }

                if (validationErrors.Any())
                {
                    throw new BuilderException(string.Join(Environment.NewLine, validationErrors));
                }

                return new TestClass(A, B, C);
            }
        }

        public class BuilderException : Exception
        {
            public BuilderException(string message) : base(message)
            {
            }
        }
    }
}");
        }[Test]
        public void ExplicitConstructorGiven()
        {
            var source = @"using System;

namespace TestSamples
{
    [BuilderPattern]
    public partial class TestClass
    {
        public TestClass(string a, string b, int c)
        {
            A = a;
            B = b;
            C = c;
        }

        public string A { get; }
        public string B { get; }
        public int    C { get; }
    }
}
";

            var (compilation, diagnostics) = RunGenerator(source);

            Assert.That(diagnostics, Is.Empty);

            var programText = GetSourceFile(compilation, "TestClass.g.cs")
                .GetText()
                .ToString();

            programText.ShouldBe(
                @"using System;

namespace TestSamples
{
    partial class TestClass
    {
        public class Builder
        {
            public string A { get; set; }

            public string B { get; set; }

            public int C { get; set; }

            public TestClass Build()
            {
                var validationFailures = new System.Collections.Generic.List<string>();

                if (A == null)
                {
                    validationFailures.Add(""A is null"");
                }

                if (B == null)
                {
                    validationFailures.Add(""B is null"");
                }

                if (validationFailures.Count > 0)
                {
                    throw new BuilderException(validationFailures);
                }

                return new TestClass(A, B, C);
            }
        }

        public class BuilderException : System.Exception
        {
            public BuilderException(params string[] validationFailures) : base(GetMessage(validationFailures))
            {
            }

            private static string GetMessage(System.Collections.Generic.IEnumerable<string> validationFailures)
            {
                return string.Join(System.Environment.NewLine, validationFailures);
            }
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
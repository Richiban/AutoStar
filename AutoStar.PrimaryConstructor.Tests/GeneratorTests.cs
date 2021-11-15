using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using AutoStar.PrimaryConstructor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Shouldly;

namespace AutoStar.PimaryConstructor.Tests
{
    [TestFixture]
    internal class GeneratorTests
    {
        [Test]
        public void InjectedAttributeFileTest()
        {
            var source = @"
[PrimaryConstructor]
public partial class TestClass
{
    private readonly string _data;
}
";

            var (outputCompilation, diagnostics) = RunGenerator(source);

            diagnostics.ShouldBeEmpty();

            var fileNames = outputCompilation.SyntaxTrees.Select(s => s.FilePath);

            outputCompilation.SyntaxTrees.Count().ShouldBe(3,
                $"We expected four syntax trees: the original one plus the two we generated. Found: {string.Join(",\n", fileNames)}");

            var cmdrAttributeFile = GetSourceFile(outputCompilation, "PrimaryConstructorAttribute.g.cs");

            var src = cmdrAttributeFile.GetText().ToString();

            src.ShouldBe(
                @"using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PrimaryConstructorAttribute : Attribute
{}");
        }

        [Test]
        public void NonPartialClassGivesDiagnosticError()
        {
            var source = @"
using System;

namespace TestSamples
{
    [PrimaryConstructor]
    public class TestClass
    {
        private readonly string _data;
    }
}
";

            var (_, diagnostics) = RunGenerator(source);

            var diagnostic = diagnostics.ShouldHaveSingleItem();
            diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
            diagnostic.Id.ShouldBe("ASPC0001");

            diagnostic.GetMessage().ShouldBe("Class TestClass must be partial");
        }

        [Test]
        public void ClassWithExistingConstructorGivesDiagnosticError()
        {
            var source = @"
using System;

namespace TestSamples
{
    [PrimaryConstructor]
    public partial class TestClass
    {
        public TestClass() {}
    }
}
";

            var (_, diagnostics) = RunGenerator(source);

            var diagnostic = diagnostics.ShouldHaveSingleItem();
            diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
            diagnostic.Id.ShouldBe("ASPC0002");

            diagnostic.GetMessage().ShouldBe("Class TestClass must not define any constructors");
        }

        [Test]
        public void DummyTest()
        {
            var source = @"
using System;

namespace TestSamples
{
    [PrimaryConstructor]
    public partial class TestClass
    {
        private readonly string _data;
    }
}
";

            var (compilation, diagnostics) = RunGenerator(source);

            Assert.That(diagnostics, Is.Empty);

            var programText = GetSourceFile(compilation, "TestClass.g.cs").GetText().ToString();

            programText.ShouldBe(
                @"using System;

namespace TestSamples
{
    public partial class TestClass
    {
        public TestClass(string data)
        {
            _data = data;
        }
    }
}
");
        }

        private static SyntaxTree GetSourceFile(Compilation outputCompilation, string fileName)
        {
            return outputCompilation.SyntaxTrees.Single(s => s.FilePath.EndsWith(fileName));
        }

        private static (Compilation, ImmutableArray<Diagnostic>) RunGenerator(
            string source)
        {
            var inputCompilation = CSharpCompilation.Create(
                "compilation",
                new[] {CSharpSyntaxTree.ParseText(source)},
                new[]
                {
                    MetadataReference.CreateFromFile(
                        typeof(Binder).GetTypeInfo().Assembly.Location)
                },
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

            var generator = new PrimaryConstructorGenerator();

            var driver = CSharpGeneratorDriver.Create(generator)
                .RunGeneratorsAndUpdateCompilation(
                    inputCompilation,
                    out var outputCompilation,
                    out var diagnostics);

            return (outputCompilation, diagnostics);
        }
    }
}
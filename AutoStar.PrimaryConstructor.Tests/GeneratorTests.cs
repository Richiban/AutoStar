using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Linq;
using Shouldly;
using System.Threading.Tasks;
using AutoStar.PrimaryConstructor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace Richiban.Cmdr.Tests
{
    [TestFixture]
    internal class GeneratorTests
    {
        [Test]
        public void CmdrAttributeFileTest()
        {
            var source = @"
[PrimaryConstructor]
public partial class TestClass
{
    private readonly string _data;
}
";

            var (outputCompilation, diagnostics) = RunGenerator(source);

            Assert.That(diagnostics, Is.Empty);

            {
                var fileNames = string.Join(
                    ",\n",
                    outputCompilation.SyntaxTrees.Select(s => s.FilePath));

                Assert.That(
                    outputCompilation.SyntaxTrees.Count,
                    Is.EqualTo(expected: 4),
                    $"We expected four syntax trees: the original one plus the three we generated. Found: {fileNames}");
            }

            var cmdrAttributeFile = GetSourceFile(outputCompilation, "PrimaryConstructorAttribute.g.cs");

            var src = cmdrAttributeFile.GetText().ToString();

            src.ShouldBe(
                @"using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class PrimaryConstructorAttribute : Attribute
{
    public PrimaryConstructorAttribute()
    {
    }
}");
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

            Assert.That(diagnostics, Has.Length.EqualTo(expected: 1));
            var diagnostic = diagnostics.Single();
            Assert.That(diagnostic.Severity, Is.EqualTo(DiagnosticSeverity.Error));
            Assert.That(diagnostic.Id, Is.EqualTo("Cmdr0001"));

            Assert.That(
                diagnostic.GetMessage(),
                Is.EqualTo(
                    "Method TestSamples.TestClass.TestMethod() must be static in order to use the Cmdr attribute."));
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
using System.CommandLine;
using System.CommandLine.Invocation;
using Richiban.Cmdr;

public static class Program
{
    public static int Main(string[] args)
    {
        var testMethodCommand = new Command(""explicit"")
        {
        };

        testMethodCommand.Handler = CommandHandler.Create(TestSamples.TestClass.TestMethod);

        var rootCommand = new RootCommand()
        {
            testMethodCommand
        };

        if (Repl.IsCall(args))
        {
            Repl.EnterNewLoop(rootCommand, ""Select a command"");

            return 0;
        }
        else
        {
            return rootCommand.Invoke(args);
        }
    }
}
");
        }

        private static SyntaxTree GetSourceFile(Compilation outputCompilation, string fileName)
        {
            return outputCompilation.SyntaxTrees.Single(s => s.FilePath == fileName);
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
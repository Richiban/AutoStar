using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Linq;
using AutoStar.PrimaryConstructor;
using AutoStar.Tests.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Shouldly;

namespace AutoStar.PimaryConstructor.Tests
{
    [TestFixture]
    internal class GeneratorTests
    {
        private readonly GeneratorTestRunner _testRunner =
            new(new PrimaryConstructorSourceGenerator());

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

            var runResult = _testRunner.RunGenerator(source);

            runResult.Diagnostics.ShouldBeEmpty();

            var fileNames =
                runResult.OutputCompilation.SyntaxTrees.Select(s => s.FilePath);

            runResult.OutputCompilation.SyntaxTrees.Count()
                .ShouldBe(
                    expected: 3,
                    $"We expected three syntax trees: the original one plus the two we generated. Found: {string.Join(",\n", fileNames)}");

            var cmdrAttributeFile = runResult.GetSourceFile(
                "PrimaryConstructorAttribute.g.cs");

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

            var (_, diagnostics) = _testRunner.RunGenerator(source);

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

            var (_, diagnostics) = _testRunner.RunGenerator(source);

            var diagnostic = diagnostics.ShouldHaveSingleItem();
            diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
            diagnostic.Id.ShouldBe("ASPC0002");

            diagnostic.GetMessage()
                .ShouldBe("Class TestClass must not define any constructors");
        }

        [Test]
        public void SimplestTestCase()
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

            var runResult = _testRunner.RunGenerator(source);

            runResult.Diagnostics.ShouldBeEmpty();

            var programText = runResult.GetSourceFile("TestClass.g.cs")
                .GetText()
                .ToString();

            programText.ShouldBe(
                @"using System;

namespace TestSamples
{
    partial class TestClass
    {
        public TestClass(string data)
        {
            _data = data;
        }
    }
}");
        }
    }
}
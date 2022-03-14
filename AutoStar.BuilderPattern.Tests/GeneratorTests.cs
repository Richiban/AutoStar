using System;
using System.Linq;
using AutoStar.Tests.Common;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using Shouldly;

namespace AutoStar.BuilderPattern.Tests
{
    [TestFixture]
    internal class GeneratorTests
    {
        private readonly GeneratorTestRunner _testRunner =
            new(new BuilderPatternGenerator());

        [Test]
        public void InjectedAttributeFileTest()
        {
            var source = @"";

            var runResult = _testRunner.RunGenerator(source);

            runResult.Diagnostics.ShouldBeEmpty();

            var src = runResult.GetSourceFile("BuilderPatternAttribute.g.cs")
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

            var (_, diagnostics) = _testRunner.RunGenerator(source);

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

            var (compilation, diagnostics) = _testRunner.RunGenerator(source);

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

            var runResult = _testRunner.RunGenerator(source);

            runResult.Diagnostics.ShouldBeEmpty();

            var programText = runResult.GetSourceFile("TestClass.g.cs")
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
        }

        [Test]
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

            var runResult = _testRunner.RunGenerator(source);

            runResult.Diagnostics.ShouldBeEmpty();

            var actual = runResult.GetSourceFile("TestClass.g.cs")
                .GetText()
                .ToString();

            var expected = (@"using System;

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

                if (A is null)
                {
                    validationFailures.Add(""A is null"");
                }

                if (B is null)
                {
                    validationFailures.Add(""B is null"");
                }

                if (C is null)
                {
                    validationFailures.Add(""C is null"");
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
            public BuilderException(System.Collections.Generic.List<string> validationFailures) : base(GetMessage(validationFailures))
            {
            }

            private static string GetMessage(System.Collections.Generic.IEnumerable<string> validationFailures)
            {
                return string.Join(""; "", validationFailures);
            }
        }
    }
}");

            CodeDiffer.AssertEqual(expected, actual);
        }
    }
}
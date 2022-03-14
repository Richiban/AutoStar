using System;
using System.Collections.Immutable;
using System.Reflection;
using System.Linq;
using AutoStar.Tests.Common;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using Shouldly;

namespace AutoStar.EnumClass.Tests
{
    [TestFixture]
    internal class GeneratorTests
    {
        private readonly GeneratorTestRunner _generatorTestRunner =
            new(new EnumClassSourceGenerator());

        [Test]
        public void InjectedAttributeFileTest()
        {
            var source = @"
[EnumClass]
public partial class TestClass
{
}
";

            var runResult =
                _generatorTestRunner.RunGenerator(source);

            runResult.Diagnostics.ShouldBeEmpty();

            var src = runResult.GetSourceFile("EnumClassAttribute.cs")
                .GetText()
                .ToString();

            src.ShouldBe(
                @"using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EnumClassAttribute : Attribute
{}");
        }

        [Test]
        public void EnumClassWithNoInnerClassesResultsInNoGeneratedFile()
        {
            var source = @"
[EnumClass]
public partial class TestClass
{
}
";

            var runResult =
                _generatorTestRunner.RunGenerator(source);

            runResult.Diagnostics.ShouldBeEmpty();

            runResult.OutputCompilation.SyntaxTrees.Count().ShouldBe(expected: 2);

            runResult.OutputCompilation.SyntaxTrees.Select(s => s.FilePath)
                .ShouldBe(
                    new[]
                    {
                        "",
                        @"AutoStar.EnumClass\AutoStar.EnumClass.EnumClassGenerator\EnumClassAttribute.cs"
                    });
        }

        [Test]
        public void NonPartialClassGivesDiagnosticError()
        {
            var source = @"
using System;

namespace TestSamples
{
    [EnumClass]
    public class TestClass
    {
        class InnerClass
        {
        }
    }
}
";

            var (_, diagnostics) = _generatorTestRunner.RunGenerator(source);

            var diagnostic = diagnostics.ShouldHaveSingleItem();
            diagnostic.Id.ShouldBe("ASEC0001");
            diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);

            diagnostic.GetMessage()
                .ShouldBe(
                    "The class TestClass must be partial in order to use the EnumClass attribute");
        }

        [Test]
        public void ClassWithExistingConstructorGivesDiagnosticError()
        {
            var source = @"
using System;

namespace TestSamples
{
    [EnumClass]
    public partial class TestClass
    {
        public TestClass() {}

        class InnerClass
        {
        }
    }
}
";

            var (_, diagnostics) = _generatorTestRunner.RunGenerator(source);

            var diagnostic = diagnostics.ShouldHaveSingleItem();
            diagnostic.Severity.ShouldBe(DiagnosticSeverity.Error);
            diagnostic.Id.ShouldBe("ASEC0002");

            diagnostic.GetMessage()
                .ShouldBe(
                    "The class TestClass must not define any constructors in order to use the EnumClass attribute");
        }

        [Test]
        public void BasicScenario()
        {
            var source = @"
namespace TestSamples
{
    [EnumClass]
    public partial class TestClass
    {
        partial class TypeA {}
        partial class TypeB {}
    }
}
";

            var runResult = _generatorTestRunner.RunGenerator(source);

            Assert.That(runResult.Diagnostics, Is.Empty);

            var programText = runResult.GetSourceFile("TestClass.g.cs")
                .GetText()
                .ToString();

            programText.ShouldBe(
                @"namespace TestSamples
{
    public abstract partial class TestClass
    {
        private TestClass()
        {
        }

        public sealed partial class TypeA : TestClass
        {
        }

        public sealed partial class TypeB : TestClass
        {
        }
    }
}");
        }

        [Test]
        public void NestedScenario()
        {
            var source = @"
namespace TestSamples
{
    [EnumClass]
    public partial class TestClass
    {
        partial class TypeA { public string PropA { get; set; } }
        partial class TypeB {}

        [EnumClass]
        partial class TypeC
        {
            partial class TypeD {}
            partial class TypeE { public string PropE { get; set; } }
        }
    }
}
";

            var runResult = _generatorTestRunner.RunGenerator(source);

            Assert.That(runResult.Diagnostics, Is.Empty);

            var programText = runResult.GetSourceFile("TestClass.g.cs")
                .GetText()
                .ToString();

            var expected = @"namespace TestSamples
{
    public abstract partial class TestClass
    {
        private TestClass()
        {
        }

        public override bool Equals(object other) => other is TestClass otherClass && Equals(otherClass);
        public bool Equals(TestClass other)
        {
            return other != null;
        }

        public override string ToString()
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            stringBuilder.Append(""TestClass { "");
            stringBuilder.Append("" }"");
            return stringBuilder.ToString();
        }

        public sealed partial class TypeA : TestClass
        {
        }

        public sealed partial class TypeB : TestClass
        {
        }

        public abstract partial class TypeC : TestClass
        {
            private TypeC()
            {
            }

            public override bool Equals(object other) => other is TypeC otherClass && Equals(otherClass);
            public bool Equals(TypeC other)
            {
                return other != null;
            }

            public override string ToString()
            {
                System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                stringBuilder.Append(""TypeC { "");
                stringBuilder.Append("" }"");
                return stringBuilder.ToString();
            }

            public sealed partial class TypeD : TypeC
            {
            }

            public sealed partial class TypeE : TypeC
            {
            }
        }
    }
}";

            CodeDiffer.AssertEqual(expected, programText);
        }
    }
}
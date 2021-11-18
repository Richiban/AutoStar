using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using Shouldly;

namespace AutoStar.Common.Tests
{
    [TestFixture]
    public class EqualsGeneratorTests
    {
        [Test]
        public void TestObjectEqualsMethod()
        {
            var src = @"
public abstract class TestClass
{
    public int TestProperty { get; set; }
    public string TestField;
    public string TestMethod()
    {
        return ""Test"";
    }
}";

            var tree = CSharpSyntaxTree.ParseText(src);

            var input = tree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single();

            var (objectEqualsMethod, _) =
                EqualsGenerator.GenerateEqualsMethodForClass(input);

            var actual = objectEqualsMethod.NormalizeWhitespace().ToFullString();

            var expected =
                @"public override bool Equals(object other) => other is TestClass otherClass && Equals(otherClass);";

            actual.ShouldBe(expected);
        }

        [Test]
        public void TestEquatableEqualsMethod()
        {
            var src = @"
public abstract class TestClass
{
    public int TestAutoProperty { get; set; }
    public object TestCustomProperty => """";
    public string TestField;
    public string TestMethod()
    {
        return ""Test"";
    }
}";

            var tree = CSharpSyntaxTree.ParseText(src);

            var input = tree.GetRoot()
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single();

            var (_, equatableEqualsMethod) =
                EqualsGenerator.GenerateEqualsMethodForClass(input);

            var actual = equatableEqualsMethod.NormalizeWhitespace().ToFullString();

            var expected = @"public bool Equals(TestClass other)
{
    return other != null && this.TestAutoProperty.Equals(other.TestAutoProperty) && this.TestField.Equals(other.TestField);
}";

            actual.ShouldBe(expected);
        }
    }
}
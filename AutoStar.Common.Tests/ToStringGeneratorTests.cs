using System.Linq;
using AutoStar.Tests.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace AutoStar.Common.Tests
{
    [TestFixture]
    public class ToStringGeneratorTests
    {
        [Test]
        public void TestMethod()
        {
            var src = @"
public class TestClass
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

            var actual = ToStringGenerator.GenerateToStringMethodForClass(input)
                .NormalizeWhitespace()
                .ToFullString();

            var expected = @"public override string ToString()
{
    System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
    stringBuilder.Append(""TestClass { "");
    stringBuilder.Append(""TestProperty = "");
    stringBuilder.Append(this.TestProperty);
    stringBuilder.Append("", "");
    stringBuilder.Append(""TestField = "");
    stringBuilder.Append(this.TestField);
    stringBuilder.Append("" }"");
    return stringBuilder.ToString();
}";

            CodeDiffer.AssertEqual(expected, actual);
        }
    }
}
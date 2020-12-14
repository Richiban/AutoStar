using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;
using AutoStar.BuilderPattern;
using System;
using System.Linq;

namespace AutoStar.Tests
{
    public class BuilderPatternGeneratorTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test1()
        {
            var sut = new BuilderPatternGenerator();
            var tree = CSharpSyntaxTree.ParseText(sampleCode);
            var root = (CompilationUnitSyntax)tree.GetRoot();
            var usings = root.Usings.ToString();

            var ns = root.ChildNodes().OfType<NamespaceDeclarationSyntax>().Single();

            var classDeclaration = ns.ChildNodes().OfType<ClassDeclarationSyntax>().Single();

            //var result = sut.GeneratePatternFor(classDeclaration, usings, ns.Name.ToString());

            //Console.WriteLine(result);
        }

        private const string sampleCode = @"
using System;

namespace Test
{
    public partial class RecordSubject
    {
        public string A { get; }
        public string? B { get; }
        public string C { get; } = """";
    }
}
"
    ;
    }

}
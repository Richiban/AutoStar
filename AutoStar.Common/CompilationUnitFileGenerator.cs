using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.Common
{
    public class CompilationUnitFileGenerator : ICodeFileGenerator
    {
        private readonly CompilationUnitSyntax _compilationUnit;

        public CompilationUnitFileGenerator(CompilationUnitSyntax compilationUnit)
        {
            _compilationUnit = compilationUnit;
            FileName = GetFileName(compilationUnit);
        }

        public string GetCode() => _compilationUnit.NormalizeWhitespace().ToFullString();

        public string FileName { get; }

        private static string GetFileName(CompilationUnitSyntax compilationUnit) =>
            compilationUnit.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .Single()
                .Identifier + ".g.cs";
    }
}
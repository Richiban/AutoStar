using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using AutoStar.BuilderPattern.Common;
using System.Linq;

namespace AutoStar.BuilderPattern
{
    [Generator]
    public class BuilderPatternGenerator : CodePatternSourceGeneratorBase
    {
        protected override string GeneratorName => nameof(BuilderPattern);

        public override GeneratedFile GeneratePatternFor(
            INamedTypeSymbol classSymbol,
            string usings,
            string @namespace)
        {
            var converter = new RecordConverter();

            var record = converter.ClassToRecord(
                (ClassDeclarationSyntax)classSymbol.DeclaringSyntaxReferences.First()
                    .GetSyntax());

            var recordClass = converter.LowerRecordToClass(record);

            var typeFile = new TypeFile(usings, recordClass)
            {
                NamespaceName = @namespace
            };

            return new GeneratedFile(typeFile, GeneratorName);
        }
    }
}
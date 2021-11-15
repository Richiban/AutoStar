using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using AutoStar.BuilderPattern.Common;

namespace AutoStar.BuilderPattern
{
    [Generator]
    public class BuilderPatternGenerator : CodePatternSourceGeneratorBase
    {
        protected override string GeneratorName => nameof(BuilderPattern);

        public override GeneratedFile GeneratePatternFor(INamedTypeSymbol classSymbol, string usings, string @namespace)
        {
            var converter = new RecordConverter();

            throw new NotImplementedException();

            //var record = converter.ClassToRecord(classSymbol.);

            //var recordClass = converter.LowerRecordToClass(record);

            //var typeFile = new TypeFile(usings, recordClass)
            //{
            //    NamespaceName = @namespace
            //};

            //return new GeneratedFile(typeFile, GeneratorName);
        }
    }
}

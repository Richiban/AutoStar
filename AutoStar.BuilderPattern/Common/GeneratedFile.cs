﻿using System;
using System.Text;
using AutoStar.BuilderPattern.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace AutoStar.BuilderPattern.Common
{
    public class GeneratedFile
    {
        public GeneratedFile(TypeFile typeFile, string generatorName)
        {
            TypeFile = typeFile;
            GeneratorName = generatorName;
        }

        public string GeneratorName { get; }
        public TypeFile TypeFile { get; }

        public Compilation AddTo(GeneratorExecutionContext context)
        {
            var options =
                ((CSharpCompilation)context.Compilation).SyntaxTrees[0].Options as
                CSharpParseOptions;

            var typeName = TypeFile.TypeDeclaration.Name;

            var codeBuilder = new CodeBuilder();

            TypeFile.WriteTo(codeBuilder);

            context.AddSource(
                $"{typeName}.{GeneratorName}.g.cs",
                SourceText.From(codeBuilder.ToString(), Encoding.UTF8));

            return context.Compilation.AddSyntaxTrees(
                CSharpSyntaxTree.ParseText(
                    SourceText.From(codeBuilder.ToString(), Encoding.UTF8),
                    options));
        }
    }
}
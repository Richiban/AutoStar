﻿using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AutoStar.PrimaryConstructor
{
    internal static class ContextExtensions
    {
        public static void AddCodeFile(
            this GeneratorExecutionContext context,
            CodeFileGenerator codeFileGenerator) =>
            context.AddSource(
                codeFileGenerator.FileName,
                SourceText.From(codeFileGenerator.GetCode(), Encoding.UTF8));
    }
}
using System;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AutoStar.Common
{
    public static class ContextExtensions
    {
        public static void AddCodeFile(
            this GeneratorExecutionContext context,
            ICodeFileGenerator codeFileGenerator) =>
            context.AddSource(
                codeFileGenerator.FileName,
                SourceText.From(codeFileGenerator.GetCode(), Encoding.UTF8));
    }
}
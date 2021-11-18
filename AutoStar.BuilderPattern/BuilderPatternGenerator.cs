﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoStar.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.BuilderPattern
{
    [Generator]
    public class BuilderPatternGenerator : ISourceGenerator
    {
        private MarkerAttribute _attribute = null!;

        public void Initialize(GeneratorInitializationContext context)
        {
            _attribute = new MarkerAttribute("BuilderPattern");

            context.RegisterForPostInitialization(InjectStaticSourceFiles);

            context.RegisterForSyntaxNotifications(() => _attribute.SyntaxReceiver);
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var diagnostics = new CustomDiagnostics(context);

            try
            {
                if (context.SyntaxReceiver is null || _attribute.MarkedClasses.Count == 0)
                {
                    return;
                }

                var (newFiles, failures) = new Scanner(_attribute, context.Compilation)
                    .BuildFor(_attribute.MarkedClasses)
                    .SeparateResults();

                diagnostics.ReportMethodFailures(failures);

                foreach (var newFile in newFiles)
                {
                    context.AddCodeFile(new CompilationUnitFileGenerator(newFile));
                }
            }
            catch (Exception ex)
            {
                diagnostics.ReportUnknownError(ex);
            }
        }

        private void InjectStaticSourceFiles(
            GeneratorPostInitializationContext postInitializationContext)
        {
            postInitializationContext.AddSource(
                _attribute.FileName,
                _attribute.GetCode());
        }
    }

    public class Scanner
    {
        public Scanner(MarkerAttribute attribute, Compilation contextCompilation)
        {
        }

        public IEnumerable<ResultOption<ScanFailure, CompilationUnitSyntax>> BuildFor(
            IEnumerable<ClassDeclarationSyntax> attributeMarkedClasses) =>
            attributeMarkedClasses.Select(BuildFor);

        public ResultOption<ScanFailure, CompilationUnitSyntax> BuildFor(
            ClassDeclarationSyntax attributeMarkedClass)
        {
            throw new NotImplementedException();
        }
    }

    public class ScanFailure
    {
    }
}
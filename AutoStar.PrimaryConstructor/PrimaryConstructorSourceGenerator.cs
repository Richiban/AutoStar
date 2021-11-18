using System;
using System.Collections.Generic;
using AutoStar.Common;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.PrimaryConstructor
{
    [Generator]
    public class PrimaryConstructorSourceGenerator : ISourceGenerator
    {
        private MarkerAttribute _attribute = null!;

        public void Initialize(GeneratorInitializationContext context)
        {
            _attribute = new MarkerAttribute("PrimaryConstructor");

            context.RegisterForPostInitialization(InjectStaticSourceFiles);

            context.RegisterForSyntaxNotifications(
                () => new SyntaxReceiver(_attribute));
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var diagnostics = new CustomDiagnostics(context);

            try
            {
                if (context.SyntaxReceiver is not SyntaxReceiver receiver ||
                    receiver.CandidateClasses.Count == 0)
                {
                    return;
                }

                var (units, failures) =
                    new Scanner(_attribute, context.Compilation)
                        .BuildFrom(receiver.CandidateClasses)
                        .SeparateResults();

                diagnostics.ReportMethodFailures(failures);

                foreach (var compilationUnit in units)
                {
                    context.AddCodeFile(
                        new CompilationUnitFileGenerator(compilationUnit));
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

        internal class SyntaxReceiver : ISyntaxReceiver
        {
            private readonly MarkerAttribute _attribute;

            public SyntaxReceiver(MarkerAttribute attribute)
            {
                _attribute = attribute;
            }

            public IList<ClassDeclarationSyntax> CandidateClasses { get; } =
                new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
                {
                    var attrs = classDeclarationSyntax.AttributeLists
                        .SelectMany(x => x.Attributes)
                        .ToList();

                    if (attrs.Any(_attribute.Matches))
                    {
                        CandidateClasses.Add(classDeclarationSyntax);
                    }
                }
            }
        }
    }
}
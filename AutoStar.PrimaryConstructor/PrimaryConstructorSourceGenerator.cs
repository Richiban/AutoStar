using System;
using System.Collections.Generic;
using System.Linq;
using AutoStar.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.PrimaryConstructor
{
    [Generator]
    public class PrimaryConstructorSourceGenerator : ISourceGenerator
    {
        private MarkerAttributeDefinition _attributeDefinition = null!;

        public void Initialize(GeneratorInitializationContext context)
        {
            _attributeDefinition = new MarkerAttributeDefinition("PrimaryConstructor");

            context.RegisterForPostInitialization(InjectStaticSourceFiles);

            context.RegisterForSyntaxNotifications(
                () => new SyntaxReceiver(_attributeDefinition));
        }

        private void InjectStaticSourceFiles(
            GeneratorPostInitializationContext postInitializationContext)
        {
            postInitializationContext.AddSource(
                _attributeDefinition.FileName,
                _attributeDefinition.GetCode());
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

                var (units, failures) = new Scanner(_attributeDefinition, context.Compilation)
                    .BuildFrom(receiver.CandidateClasses)
                    .SeparateResults();

                diagnostics.ReportMethodFailures(failures);

                foreach (var compilationUnit in units)
                {
                    context.AddCodeFile(new CompilationUnitFileGenerator(compilationUnit));
                }
            }
            catch (Exception ex)
            {
                diagnostics.ReportUnknownError(ex);
            }
        }

        internal class SyntaxReceiver : ISyntaxReceiver
        {
            private readonly MarkerAttributeDefinition _attributeDefinition;

            public SyntaxReceiver(MarkerAttributeDefinition attributeDefinition)
            {
                _attributeDefinition = attributeDefinition;
            }

            public IList<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
                {
                    var attrs = classDeclarationSyntax.AttributeLists
                        .SelectMany(x => x.Attributes)
                        .ToList();

                    if (attrs.Any(_attributeDefinition.Matches)) CandidateClasses.Add(classDeclarationSyntax);
                }
            }
        }
    }
}
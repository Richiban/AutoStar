using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.PrimaryConstructor
{
    [Generator]
    public class PrimaryConstructorGenerator : ISourceGenerator
    {
        private PrimaryConstructorAttributeDefinition _cmdrAttribute = null!;

        public void Initialize(GeneratorInitializationContext context)
        {
            _cmdrAttribute = new PrimaryConstructorAttributeDefinition();

            context.RegisterForPostInitialization(InjectStaticSourceFiles);

            context.RegisterForSyntaxNotifications(
                () => new SyntaxReceiver(_cmdrAttribute));
        }

        private void InjectStaticSourceFiles(
            GeneratorPostInitializationContext postInitializationContext)
        {
            var cmdrAttributeFileGenerator =
                new AttributeFileGenerator(_cmdrAttribute);

            postInitializationContext.AddSource(
                cmdrAttributeFileGenerator.FileName,
                cmdrAttributeFileGenerator.GetCode());
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

                var (models, failures) = new ModelBuilder(_cmdrAttribute, context.Compilation)
                    .BuildFrom(receiver.CandidateClasses)
                    .SeparateResults();

                diagnostics.ReportMethodFailures(failures);

                foreach (var model in models)
                {
                    context.AddCodeFile(new PrimaryConstructorClassFileGenerator(model));
                }
            }
            catch (Exception ex)
            {
                diagnostics.ReportUnknownError(ex);
            }
        }

        internal class SyntaxReceiver : ISyntaxReceiver
        {
            private readonly PrimaryConstructorAttributeDefinition _attributeDefinition;

            public SyntaxReceiver(PrimaryConstructorAttributeDefinition attributeDefinition)
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
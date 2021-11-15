using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.EnumClass
{
    [Generator]
    public class EnumClassGenerator : ISourceGenerator
    {
        private EnumClassAttributeDefinition _attributeDefinition = null!;

        public void Initialize(GeneratorInitializationContext context)
        {
            _attributeDefinition = new EnumClassAttributeDefinition();

            context.RegisterForPostInitialization(InjectStaticSourceFiles);

            context.RegisterForSyntaxNotifications(
                () => new SyntaxReceiver(_attributeDefinition));
        }

        private void InjectStaticSourceFiles(
            GeneratorPostInitializationContext postInitializationContext)
        {
            var attributeFileGenerator =
                new AttributeFileGenerator(_attributeDefinition);

            postInitializationContext.AddSource(
                attributeFileGenerator.FileName,
                attributeFileGenerator.GetCode());
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

                var (models, failures) = new ModelBuilder(_attributeDefinition, context.Compilation)
                    .BuildFrom(receiver.CandidateClasses)
                    .SeparateResults();

                diagnostics.ReportMethodFailures(failures);

                foreach (var model in models)
                {
                    context.AddCodeFile(new EnumClassFileGenerator(model));
                }
            }
            catch (Exception ex)
            {
                diagnostics.ReportUnknownError(ex);
            }
        }

        internal class SyntaxReceiver : ISyntaxReceiver
        {
            private readonly EnumClassAttributeDefinition _attributeDefinition;

            public SyntaxReceiver(EnumClassAttributeDefinition attributeDefinition)
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
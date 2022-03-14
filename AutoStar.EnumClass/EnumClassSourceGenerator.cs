using AutoStar.Common;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.EnumClass
{
    [Generator]
    public class EnumClassSourceGenerator : ISourceGenerator
    {
        private readonly MarkerAttribute _attribute = new MarkerAttribute("EnumClass");

        private readonly SwitchSyntaxReceiver _switchSyntaxReceiver =
            new SwitchSyntaxReceiver();

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForPostInitialization(InjectStaticSourceFiles);

            context.RegisterForSyntaxNotifications(() => _attribute);
            context.RegisterForSyntaxNotifications(() => _switchSyntaxReceiver);
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

                var (models, failures) =
                    new EnumClassBuilder(_attribute, context.Compilation)
                        .BuildFor(_attribute.MarkedClasses)
                        .SeparateResults();

                diagnostics.ReportFailures(failures);

                foreach (var model in models)
                {
                    context.AddCodeFile(new EnumClassFileGenerator(model));
                }

                var switchStatementFailures = new SwitchStatementAnalyzer(
                    context.Compilation,
                    _attribute,
                    _switchSyntaxReceiver).Analyze();

                diagnostics.ReportFailures(switchStatementFailures);
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

    internal class SwitchSyntaxReceiver : ISyntaxReceiver
    {
        private readonly List<SwitchStatementSyntax> _switchStatements = new();
        private readonly List<SwitchExpressionSyntax> _switchExpressions = new();

        public IReadOnlyList<SwitchStatementSyntax> SwitchStatements => _switchStatements;

        public IReadOnlyList<SwitchExpressionSyntax> SwitchExpressions =>
            _switchExpressions;

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            switch (syntaxNode)
            {
                case SwitchStatementSyntax switchStatement:
                    _switchStatements.Add(switchStatement);

                    break;
                case SwitchExpressionSyntax switchExpression:
                    _switchExpressions.Add(switchExpression);

                    break;
            }
        }
    }
}
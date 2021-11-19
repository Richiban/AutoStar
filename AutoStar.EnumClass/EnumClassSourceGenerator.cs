using AutoStar.Common;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace AutoStar.EnumClass
{
    [Generator]
    public class EnumClassSourceGenerator : ISourceGenerator
    {
        private MarkerAttribute _attribute = null!;

        public void Initialize(GeneratorInitializationContext context)
        {
            _attribute = new MarkerAttribute("EnumClass");

            context.RegisterForPostInitialization(InjectStaticSourceFiles);

            context.RegisterForSyntaxNotifications(() => _attribute);
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

                var (models, failures) = new Scanner(_attribute, context.Compilation)
                    .BuildFor(_attribute.MarkedClasses)
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

        private void InjectStaticSourceFiles(
            GeneratorPostInitializationContext postInitializationContext)
        {
            postInitializationContext.AddSource(
                _attribute.FileName,
                _attribute.GetCode());
        }
    }
}
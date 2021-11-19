using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace AutoStar.BuilderPattern
{
    class CustomDiagnostics
    {
        private readonly GeneratorExecutionContext _context;

        public CustomDiagnostics(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public void ReportMethodFailures(IEnumerable<CodeGenerationFailure> failures)
        {
            foreach (var scanFailure in failures)
            {
                ReportMethodFailure(scanFailure);
            }
        }

        private void ReportMethodFailure(CodeGenerationFailure failure)
        {
            _context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        failure.ErrorId.ToString(),
                        "Failed to register method",
                        failure.Message,
                        "Cmdr",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    failure.Location));
        }

        public void ReportUnknownError(Exception exception)
        {
            _context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "Cmdr0004",
                        "Unhandled exception",
                        $"{exception}",
                        "Cmdr",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    location: null));
        }
    }
}
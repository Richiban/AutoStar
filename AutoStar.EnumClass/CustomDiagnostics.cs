using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace AutoStar.EnumClass
{
    internal class CustomDiagnostics
    {
        private readonly GeneratorExecutionContext _context;

        public CustomDiagnostics(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public void ReportModelFailure(ModelFailure failure)
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

        public void ReportMethodFailures(IEnumerable<ModelFailure> failures)
        {
            foreach (var failure in failures)
            {
                ReportModelFailure(failure);
            }
        }

        public void ReportUnknownError(Exception ex)
        {
            _context.ReportDiagnostic(
                Diagnostic.Create(
                    new DiagnosticDescriptor(
                        "Cmdr0004",
                        "Unhandled exception",
                        $"There was an unhandled exception: {ex.Message}",
                        "Cmdr",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    location: null));
        }
    }
}
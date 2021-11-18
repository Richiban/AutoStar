using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace AutoStar.PrimaryConstructor
{
    internal class CustomDiagnostics
    {
        private readonly GeneratorExecutionContext _context;

        public CustomDiagnostics(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public void ReportModelFailure(ScanFailure failure)
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

        public void ReportMethodFailures(IEnumerable<ScanFailure> failures)
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
                        "There was an unhandled exception: {ex.Message}",
                        $"{ex.StackTrace}",
                        "Cmdr",
                        DiagnosticSeverity.Error,
                        isEnabledByDefault: true),
                    location: null));
        }
    }
}
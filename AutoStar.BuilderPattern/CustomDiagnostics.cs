using System;
using Microsoft.CodeAnalysis;

namespace AutoStar.BuilderPattern
{
    public class CustomDiagnostics
    {
        private readonly GeneratorExecutionContext _context;

        public CustomDiagnostics(GeneratorExecutionContext context)
        {
            _context = context;
        }

        public void ReportMethodFailures(object failures)
        {
            throw new NotImplementedException();
        }

        public void ReportUnknownError(Exception exception)
        {
            throw new NotImplementedException();
        }
    }
}
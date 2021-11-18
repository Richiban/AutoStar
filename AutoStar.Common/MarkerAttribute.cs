using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.Common
{
    public class MarkerAttribute : ISyntaxReceiver
    {
        private readonly List<ClassDeclarationSyntax> _markedClasses = new();

        public MarkerAttribute(string shortName)
        {
            ShortName = shortName;
        }

        public string ShortName { get; }
        public string LongName => ShortName + "Attribute";
        public string FileName => LongName + ".g.cs";

        public IReadOnlyList<ClassDeclarationSyntax> MarkedClasses => _markedClasses;

        public string GetCode() =>
            @$"using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class {LongName} : Attribute
{{}}";

        public bool Matches(AttributeSyntax arg) =>
            arg.Name.ToString() == LongName || arg.Name.ToString() == ShortName;

        public bool IsMatch(AttributeData arg) =>
            arg.AttributeClass?.Name == LongName || arg.AttributeClass?.Name == ShortName;

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is not ClassDeclarationSyntax classDeclarationSyntax)
            {
                return;
            }

            var attrs = classDeclarationSyntax.AttributeLists
                .SelectMany(x => x.Attributes)
                .ToList();

            if (attrs.Any(Matches))
            {
                _markedClasses.Add(classDeclarationSyntax);
            }
        }
    }
}
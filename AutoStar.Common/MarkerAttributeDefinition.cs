using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.Common
{
    public class MarkerAttributeDefinition
    {
        public MarkerAttributeDefinition(string shortName)
        {
            _shortName = shortName;
        }

        private readonly string _shortName;
        public string LongName => _shortName + "Attribute";
        public string FileName => LongName + ".g.cs";

        public string GetCode()
        {
            return @$"using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class {LongName} : Attribute
{{}}";
        }

        public bool Matches(AttributeSyntax arg) =>
            arg.Name.ToString() == LongName || arg.Name.ToString() == _shortName;

        public bool IsMatch(AttributeData arg) =>
            arg.AttributeClass?.Name == LongName || arg.AttributeClass?.Name == _shortName;
    }
}
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.EnumClass
{
    public class EnumClassAttributeDefinition
    {
        public string ShortName => "EnumClass";
        public string LongName => ShortName + "Attribute";

        public bool Matches(AttributeSyntax arg) => arg.Name.ToString() == LongName || arg.Name.ToString() == ShortName;

        public bool IsMatch(AttributeData arg) =>
            arg.AttributeClass?.Name == LongName || arg.AttributeClass?.Name == ShortName;
    }
}
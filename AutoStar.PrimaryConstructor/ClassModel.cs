using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.PrimaryConstructor
{
    public class ClassModel
    {
        public ClassModel(string className, ClassDeclarationSyntax classDeclaration,
            IReadOnlyList<FieldModel> readonlyFields)
        {
            ClassName = className;
            ClassDeclaration = classDeclaration;
            ReadonlyFields = readonlyFields;
        }

        public string ClassName { get; }
        public ClassDeclarationSyntax ClassDeclaration { get; }
        public IReadOnlyList<FieldModel> ReadonlyFields { get; }
    }

    public class FieldModel
    {
        public FieldModel(IdentifierName name, ITypeSymbol type)
        {
            Name = name;
            Type = type;
        }

        public IdentifierName Name { get; }
        public ITypeSymbol Type { get; }
    }

    public class IdentifierName
    {
        private readonly IReadOnlyList<string> _parts;

        private IdentifierName(string original, IReadOnlyList<string> parts)
        {
            _parts = parts;
            Original = original;
        }

        public string Original { get; }

        public static IdentifierName Parse(string original)
        {
            var parts = SplitName(original);

            return new(original, parts);
        }

        private static string[] SplitName(string original)
        {
            if (String.IsNullOrEmpty(original))
                return Array.Empty<string>();
            
            var words = new List<string>();

            var temp = string.Empty;

            foreach (var ch in original)
            {
                switch (ch)
                {
                    case '_': continue;
                    case >= 'a' and <= 'z':
                        temp = temp + ch;
                        break;
                    default:
                        words.Add(temp);
                        temp = string.Empty + ch;
                        break;
                }
            }

            words.Add(temp);
            
            return words.ToArray();
        }

        private static bool MatchesCase(char pc, char c)
        {
            return char.IsUpper(pc) == char.IsUpper(c);
        }

        public string ToParameterNaming() => ToCamelCase();

        private string ToCamelCase()
        {
            var sb = new StringBuilder();

            foreach (var part in _parts)
            {
                if (sb.Length == 0)
                {
                    sb.Append(part);
                }
                else
                {
                    sb.Append(char.ToUpper(part[0]));
                    sb.Append(part.Substring(1));
                }
            }

            return sb.ToString();
        }
    }
}
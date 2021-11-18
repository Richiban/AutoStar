using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.Common
{
    public class PrimaryConstructorField
    {
        private readonly Lazy<string> _parameterName;

        public PrimaryConstructorField(string fieldName, string typeName) : this(
            fieldName,
            typeName,
            SplitName(fieldName))
        {
        }

        private PrimaryConstructorField(
            string fieldName,
            string typeName,
            IReadOnlyList<string> parts)
        {
            FieldName = fieldName;
            TypeName = typeName;

            _parameterName = new Lazy<string>(() => ToCamelCase(parts));
        }

        public string FieldName { get; }
        public string TypeName { get; }

        public string ParameterName => _parameterName.Value;

        private static string[] SplitName(string original)
        {
            if (string.IsNullOrEmpty(original))
            {
                return Array.Empty<string>();
            }

            var words = new List<string>();

            var buffer = new StringBuilder();

            foreach (var c in original)
            {
                switch (c)
                {
                    case '_': continue;
                    case >= 'a' and <= 'z':
                        buffer.Append(c);

                        break;
                    default:
                        words.Add(buffer.ToString());
                        buffer.Clear();
                        buffer.Append(c);

                        break;
                }
            }

            words.Add(buffer.ToString());

            return words.ToArray();
        }

        private static string ToCamelCase(IEnumerable<string> parts)
        {
            var sb = new StringBuilder();

            foreach (var part in parts)
            {
                if (sb.Length == 0)
                {
                    sb.Append(part);
                }
                else
                {
                    sb.Append(char.ToUpper(part[index: 0]));
                    sb.Append(part.Substring(startIndex: 1));
                }
            }

            return sb.ToString();
        }

        public static ImmutableArray<PrimaryConstructorField> From(
            FieldDeclarationSyntax fieldDeclarationSyntax) =>
            fieldDeclarationSyntax.Declaration.Variables.Select(
                    x => new PrimaryConstructorField(
                        x.Identifier.Text,
                        fieldDeclarationSyntax.Declaration.Type.ToString()))
                .ToImmutableArray();
    }
}
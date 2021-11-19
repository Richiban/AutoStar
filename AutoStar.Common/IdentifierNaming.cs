using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace AutoStar.Common
{
    public class IdentifierNaming
    {
        private readonly string[] _parts;

        private IdentifierNaming(string[] parts)
        {
            _parts = parts;
        }

        public static IdentifierNaming Create(string name)
        {
            return new IdentifierNaming(SplitName(name));
        }

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

        private string? _asCamelCase;

        public string ToCamelCase()
        {
            if (_asCamelCase is null)
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
                        sb.Append(char.ToUpper(part[index: 0]));
                        sb.Append(part.Substring(startIndex: 1));
                    }
                }

                _asCamelCase = sb.ToString();
            }

            return _asCamelCase;
        }

        private string? _asPascalCase;

        public string ToPascalCase()
        {
            if (_asPascalCase is null)
            {
                var sb = new StringBuilder();

                foreach (var part in _parts)
                {
                    sb.Append(char.ToUpper(part[index: 0]));
                    sb.Append(part.Substring(startIndex: 1));
                }

                _asPascalCase = sb.ToString();
            }

            return _asPascalCase;
        }

        public static IdentifierNaming Create(SyntaxToken identifier)
        {
            return Create(identifier.ValueText);
        }
    }
}
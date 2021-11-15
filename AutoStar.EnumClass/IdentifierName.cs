using System;
using System.Collections.Generic;
using System.Text;

namespace AutoStar.EnumClass
{
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
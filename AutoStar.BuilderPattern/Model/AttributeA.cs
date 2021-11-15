using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoStar.BuilderPattern.Model
{
    public class AttributeA : IWriteableCode
    {
        public AttributeA(string attributeName)
        {
            Name = attributeName;
        }

        public string Name { get; }

        public IReadOnlyCollection<string> Arguments { get; init; } = new string[] { };

        public void WriteTo(CodeBuilder codeBuilder)
        {
            codeBuilder.Append("[");

            codeBuilder.Append(Name);

            if (Arguments.Any())
            {
                codeBuilder.Append("(");

                codeBuilder.Append(String.Join(", ", Arguments));

                codeBuilder.Append(")");
            }

            codeBuilder.AppendLine("]");
        }
    }
}

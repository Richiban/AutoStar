using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoStar.BuilderPattern.Model
{
    public class ConstructorCall : IWriteableCode
    {
        public ConstructorCall(string name, IReadOnlyCollection<string> arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        public string Name { get; }
        public IReadOnlyCollection<string> Arguments { get; }

        public void WriteTo(CodeBuilder codeBuilder)
        {
            codeBuilder.Append($"return new {Name}(");

            using (codeBuilder.Indent())
            {
                var isFirst = true;

                foreach (var prop in Arguments)
                {
                    if (!isFirst)
                        codeBuilder.Append(", ");

                    codeBuilder.Append(prop);
                    isFirst = false;
                }
            }

            codeBuilder.AppendLine($");");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoStar.BuilderPattern.Model
{
    public abstract class Constructor : IWriteableCode
    {
        private Constructor() { }

        public abstract void WriteTo(CodeBuilder codeBuilder);

        public class BlockConstructor : Constructor
        {
            public BlockConstructor(string name)
            {
                Name = name;
            }

            public string Name { get; }
            public IReadOnlyCollection<Parameter> Parameters { get; init; } = null!;
            public IReadOnlyCollection<AssignmentStatement> Statements { get; init; } = null!;
            public Visibility Visibility { get; init; } = Visibility.Public;
            public IReadOnlyCollection<string> BaseCall { get; init; } = null!;

            public override void WriteTo(CodeBuilder codeBuilder)
            {
                Visibility.WriteTo(codeBuilder);
                codeBuilder.Append(Name);
                codeBuilder.Append("(");

                var isFirst = true;
                foreach (var p in Parameters)
                {
                    if (isFirst == false)
                    {
                        codeBuilder.Append(", ");
                    }

                    p.WriteTo(codeBuilder);
                    isFirst = false;
                }

                codeBuilder.Append(")");

                if (BaseCall?.Any() == true)
                {
                    codeBuilder.Append(" : base(");

                    codeBuilder.Append(string.Join(", ", BaseCall));

                    codeBuilder.Append(")");
                }

                codeBuilder.AppendLine("");

                codeBuilder.AppendLine("{");

                using (codeBuilder.Indent())
                {
                    foreach (var statement in Statements)
                    {
                        statement.WriteTo(codeBuilder);
                    }
                }

                codeBuilder.AppendLine("}");
            }
        }

        public class None : Constructor { public override void WriteTo(CodeBuilder codeBuilder) { } }
    }
}
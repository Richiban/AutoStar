using System;
using System.Collections.Generic;

namespace AutoStar.BuilderPattern.Model
{
    public class MethodDeclaration : IWriteableCode
    {
        public MethodDeclaration(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; init; }
        public string Type { get; init; }
        public Visibility Visibility { get; init; }
        public bool IsStatic { get; init; }
        public bool IsOverride { get; init; }
        public IReadOnlyCollection<IWriteableCode> Contents { get; init; } = new IWriteableCode[] { };
        public IReadOnlyCollection<Parameter> Parameters { get; init; } = new Parameter[] { };

        public void WriteTo(CodeBuilder codeBuilder)
        {
            Visibility.WriteTo(codeBuilder);

            if (IsStatic)
            {
                codeBuilder.Append("static ");
            }

            if (IsOverride)
            {
                codeBuilder.Append("override ");
            }

            codeBuilder.Append($"{Type} {Name}(");

            codeBuilder.WriteAll(Parameters, ", ");

            codeBuilder.AppendLine($")");
            codeBuilder.AppendLine("{");

            using (codeBuilder.Indent())
            {
                foreach (var item in Contents ?? new IWriteableCode[] { })
                {
                    item.WriteTo(codeBuilder);
                }
            }

            codeBuilder.AppendLine("}");
        }
    }
}
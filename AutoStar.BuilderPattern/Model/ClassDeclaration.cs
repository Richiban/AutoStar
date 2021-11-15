using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoStar.BuilderPattern.Model
{
    public class ClassDeclaration : ITypeDeclaration
    {
        public ClassDeclaration(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public Constructor Constructor { get; init; } = new Constructor.None();
        public Visibility Visibility { get; init; } = Visibility.None;
        public bool IsPartial { get; init; } = false;
        public IReadOnlyCollection<IWriteableCode> Contents { get; init; } = new IWriteableCode[] { };
        public string? BaseClass { get; init; }
        public AttributeA Attribute { get; init; }

        public void WriteTo(CodeBuilder codeBuilder)
        {
            Visibility.WriteTo(codeBuilder);

            if (IsPartial)
                codeBuilder.Append("partial ");

            codeBuilder.Append("class ");
            codeBuilder.Append(Name);

            if (BaseClass != null)
            {
                codeBuilder.Append($" : {BaseClass}");
            }

            codeBuilder.AppendLine("");

            codeBuilder.AppendLine("{");

            using (codeBuilder.Indent())
            {
                Constructor?.WriteTo(codeBuilder);

                foreach (var item in Contents ?? new IWriteableCode[] { })
                {
                    item.WriteTo(codeBuilder);
                }
            }

            codeBuilder.AppendLine("}");
        }
    }
}
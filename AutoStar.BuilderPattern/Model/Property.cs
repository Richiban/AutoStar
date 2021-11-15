using System;

namespace AutoStar.BuilderPattern.Model
{
    public class Property : IWriteableCode
    {
        public Property(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public string Type { get; }
        public bool HasSetter { get; init; } = false;
        public Visibility Visibility { get; init; } = Visibility.Public;
        public string? DefaultValue { get; init; }

        public void WriteTo(CodeBuilder codeBuilder)
        {
            Visibility.WriteTo(codeBuilder);
            codeBuilder.Append($"{Type} {Name}");

            if (HasSetter)
                codeBuilder.Append(" { get; set; }");
            else
                codeBuilder.Append(" { get; }");

            if (DefaultValue != null)
            {
                codeBuilder.Append($" {DefaultValue};");
            }

            codeBuilder.AppendLine("");
        }
    }
}
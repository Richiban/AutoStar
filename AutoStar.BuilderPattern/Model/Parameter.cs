using System;

namespace AutoStar.BuilderPattern.Model
{
    public class Parameter : IWriteableCode
    {
        public Parameter(string name, string type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public string Type { get; }

        public void WriteTo(CodeBuilder codeBuilder)
        {
            codeBuilder.Append($"{Type} {Name}");
        }
    }
}
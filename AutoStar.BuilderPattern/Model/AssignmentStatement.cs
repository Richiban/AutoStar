using System;

namespace AutoStar.BuilderPattern.Model
{
    public class AssignmentStatement : IWriteableCode
    {
        public AssignmentStatement(string lhs, string rhs)
        {
            Lhs = lhs;
            Rhs = rhs;
        }

        public string Lhs { get; }
        public string Rhs { get; }

        public void WriteTo(CodeBuilder codeBuilder)
        {
            codeBuilder.AppendLine($"{Lhs} = {Rhs};");
        }
    }
}
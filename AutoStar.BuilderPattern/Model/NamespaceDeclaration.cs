using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AutoStar.BuilderPattern.Model
{
    public class NamespaceDeclaration : IWriteableCode
    {
        private readonly string _name;

        public NamespaceDeclaration(string name)
        {
            _name = name;
        }

        public IReadOnlyCollection<ITypeDeclaration> TypeDeclarations { get; init; }

        public void WriteTo(CodeBuilder codeBuilder)
        {
            codeBuilder.AppendLine($"namespace {_name}");
            codeBuilder.AppendLine("{");

            using (codeBuilder.Indent())
            {
                codeBuilder.WriteAll(TypeDeclarations);
            }

            codeBuilder.AppendLine("}");
        }
    }
}

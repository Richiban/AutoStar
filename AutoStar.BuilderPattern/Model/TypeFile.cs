using System;

namespace AutoStar.BuilderPattern.Model
{
    /// <summary>
    /// A representation of a C# file containing a single namespace containing a single type declaration
    /// </summary>
    public class TypeFile : IWriteableCode
    {
        public TypeFile(string usings, ITypeDeclaration typeDeclaration)
        {
            Usings = usings;
            TypeDeclaration = typeDeclaration;
        }

        public string? NamespaceName { get; init; }
        public string Usings { get; }
        public ITypeDeclaration TypeDeclaration { get; }

        public void WriteTo(CodeBuilder codeBuilder)
        {
            codeBuilder.AppendLine(Usings);

            if (NamespaceName is not null)
            {
                codeBuilder.AppendLine($"namespace {NamespaceName}");
                codeBuilder.AppendLine("{");
            }

            using (codeBuilder.Indent())
            {
                TypeDeclaration.WriteTo(codeBuilder);
            }

            if (NamespaceName is not null)
            {
                codeBuilder.AppendLine("}");
            }
        }
    }
}
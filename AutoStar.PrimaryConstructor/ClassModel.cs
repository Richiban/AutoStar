using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.PrimaryConstructor
{
    public class ClassModel
    {
        public ClassModel(string className, ClassDeclarationSyntax classDeclaration,
            IReadOnlyList<FieldModel> readonlyFields)
        {
            ClassName = className;
            ClassDeclaration = classDeclaration;
            ReadonlyFields = readonlyFields;
        }

        public string ClassName { get; }
        public ClassDeclarationSyntax ClassDeclaration { get; }
        public IReadOnlyList<FieldModel> ReadonlyFields { get; }
    }
}
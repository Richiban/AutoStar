using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.EnumClass
{
    public class EnumClassModel
    {
        public EnumClassModel(
            string className, 
            ClassDeclarationSyntax classDeclaration,
            IReadOnlyList<ClassDeclarationSyntax> innerClasses)
        {
            ClassName = className;
            ClassDeclaration = classDeclaration;
            InnerClasses = innerClasses;
        }

        public string ClassName { get; }
        public ClassDeclarationSyntax ClassDeclaration { get; }
        public IReadOnlyList<ClassDeclarationSyntax> InnerClasses { get; }
    }
}
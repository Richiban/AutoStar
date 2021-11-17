using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.EnumClass
{
    class EnumClassFileGenerator : CodeFileGenerator
    {
        private readonly EnumClassModel _model;

        public EnumClassFileGenerator(EnumClassModel model) : base(
            model.ClassName + ".g.cs")
        {
            _model = model;
        }

        public override string GetCode()
        {
            var root = _model.ClassDeclaration.SyntaxTree.GetRoot();

            var usings = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();

            var newClassSyntax =
                EnumClassConverter.ConvertEnumClass(_model.ClassDeclaration);

            if (GetNamespace(root) is { } namespaceDeclaration)
            {
                return ToFullString(
                    NamespaceWithNewClass(namespaceDeclaration, newClassSyntax, usings));
            }

            return ToFullString(newClassSyntax);
        }

        private static string ToFullString(SyntaxNode newClassSyntax) =>
            newClassSyntax.SyntaxTree.GetRoot().NormalizeWhitespace().ToFullString();

        private static NamespaceDeclarationSyntax NamespaceWithNewClass(
            NamespaceDeclarationSyntax namespaceDeclaration,
            MemberDeclarationSyntax newClassSyntax,
            IEnumerable<UsingDirectiveSyntax> usingDirectives) =>
            namespaceDeclaration.WithMembers(SyntaxFactory.List(new[] { newClassSyntax }))
                .WithUsings(SyntaxFactory.List(usingDirectives));

        private static NamespaceDeclarationSyntax? GetNamespace(SyntaxNode root) =>
            root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
    }
}
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

            var newClassSyntax = CreateNewClassSyntax();

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

        private ClassDeclarationSyntax CreateNewClassSyntax() =>
            _model.ClassDeclaration.WithMembers(GetNewMembers())
                .WithAttributeLists(new SyntaxList<AttributeListSyntax>());

        private SyntaxList<MemberDeclarationSyntax> GetNewMembers()
        {
            var memberDeclarations =
                new List<MemberDeclarationSyntax> { GetNewConstructor() };

            memberDeclarations.AddRange(GetPartialInnerClasses());

            return SyntaxFactory.List(memberDeclarations);
        }

        private IEnumerable<MemberDeclarationSyntax> GetPartialInnerClasses() =>
            _model.InnerClasses.Select(ToPartialInnerClass);

        private ClassDeclarationSyntax ToPartialInnerClass(
            ClassDeclarationSyntax classDeclarationSyntax)
        {
            TypeSyntax typeSyntax = SyntaxFactory.IdentifierName(_model.ClassName);
            
            return classDeclarationSyntax
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.SealedKeyword),
                        SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                .WithBaseList(
                    SyntaxFactory.BaseList(
                        SyntaxFactory.SeparatedList(
                            new[] { (BaseTypeSyntax)SyntaxFactory.SimpleBaseType(typeSyntax) })));
        }

        private ConstructorDeclarationSyntax GetNewConstructor() =>
            SyntaxFactory
                .ConstructorDeclaration(SyntaxFactory.Identifier(_model.ClassName))
                .WithBody(GetConstructorBody())
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));

        private BlockSyntax GetConstructorBody() => SyntaxFactory.Block();
    }
}
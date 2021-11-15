using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.PrimaryConstructor
{
    class PrimaryConstructorClassFileGenerator : CodeFileGenerator
    {
        private readonly ClassModel _model;

        public PrimaryConstructorClassFileGenerator(ClassModel model) : base(model.ClassName + ".g.cs")
        {
            _model = model;
        }

        public override string GetCode()
        {
            var root = _model.ClassDeclaration.SyntaxTree.GetRoot();

            var newClassSyntax = CreateNewClassSyntax();

            if (GetNamespace(root) is { } namespaceDeclaration)
            {
                return ToFullString(NamespaceWithNewClass(namespaceDeclaration, newClassSyntax));
            }

            return ToFullString(newClassSyntax);
        }

        private static string ToFullString(SyntaxNode newClassSyntax) =>
            newClassSyntax.SyntaxTree.GetRoot().NormalizeWhitespace().ToFullString();

        private static NamespaceDeclarationSyntax NamespaceWithNewClass(
            NamespaceDeclarationSyntax namespaceDeclaration, MemberDeclarationSyntax newClassSyntax) =>
            namespaceDeclaration.WithMembers(SyntaxFactory.List(new[]
            {
                newClassSyntax
            }));

        private static NamespaceDeclarationSyntax? GetNamespace(SyntaxNode root) =>
            root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();

        private ClassDeclarationSyntax CreateNewClassSyntax() =>
            _model.ClassDeclaration.WithMembers(GetNewMembers())
                .WithAttributeLists(new SyntaxList<AttributeListSyntax>());

        private SyntaxList<MemberDeclarationSyntax> GetNewMembers() => new(GetNewConstructor());

        private ConstructorDeclarationSyntax GetNewConstructor()
        {
            var constructorDeclarationSyntax =
                SyntaxFactory.ConstructorDeclaration(SyntaxFactory.Identifier(_model.ClassName))
                    .WithBody(SyntaxFactory.Block())
                    .WithParameterList(GetParameterList())
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

            return constructorDeclarationSyntax;
        }

        private ParameterListSyntax GetParameterList()
        {
            var parameters = _model.ReadonlyFields.Select(MapParameter).ToArray();
            
            return SyntaxFactory.ParameterList(SyntaxFactory.SeparatedList(parameters));
        }

        private static ParameterSyntax MapParameter(FieldModel field)
        {
            var identifier = SyntaxFactory.Identifier(field.Name.ToParameterNaming());
            
            return SyntaxFactory.Parameter(identifier)
                .WithType(SyntaxFactory.IdentifierName(field.Type.Name));
        }
    }
}
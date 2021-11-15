using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.PrimaryConstructor
{
    class EnumClassFileGenerator : CodeFileGenerator
    {
        private readonly EnumClassModel _model;

        public EnumClassFileGenerator(EnumClassModel model) : base(model.ClassName + ".g.cs")
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
                return ToFullString(NamespaceWithNewClass(namespaceDeclaration, newClassSyntax, usings));
            }

            return ToFullString(newClassSyntax);
        }

        private static string ToFullString(SyntaxNode newClassSyntax) =>
            newClassSyntax.SyntaxTree.GetRoot().NormalizeWhitespace().ToFullString();

        private static NamespaceDeclarationSyntax NamespaceWithNewClass(
            NamespaceDeclarationSyntax namespaceDeclaration, MemberDeclarationSyntax newClassSyntax,
            List<UsingDirectiveSyntax> usingDirectives) =>
            namespaceDeclaration.WithMembers(SyntaxFactory.List(new[]
                {
                    newClassSyntax
                }))
                .WithUsings(SyntaxFactory.List(usingDirectives));

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
                    .WithBody(GetConstructorBody())
                    .WithParameterList(GetParameterList())
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

            return constructorDeclarationSyntax;
        }

        private BlockSyntax GetConstructorBody()
        {
            var expressionStatementSyntax = _model.ReadonlyFields.Select(GetFieldAssignment);

            return SyntaxFactory.Block(SyntaxFactory.List(expressionStatementSyntax));
        }

        private ExpressionStatementSyntax GetFieldAssignment(FieldModel fieldModel)
        {
            var left = SyntaxFactory.IdentifierName(fieldModel.Name.Original);
            var right = SyntaxFactory.IdentifierName(fieldModel.Name.ToParameterNaming());

            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, left, right));
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
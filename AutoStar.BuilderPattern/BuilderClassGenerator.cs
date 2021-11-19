using System.Collections.Generic;
using AutoStar.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoStar.BuilderPattern
{
    internal static class BuilderClassGenerator
    {
        public static ClassDeclarationSyntax GenerateBuilderClass(
            string builderTargetTypeName,
            IReadOnlyList<PrimaryConstructorParameter> buildableParameters)
        {
            return ClassDeclaration("Builder")
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithMembers(
                    List(GetBuilderMembers(builderTargetTypeName, buildableParameters)));
        }

        private static MethodDeclarationSyntax GenerateBuildMethod(
            string builderTargetTypeName,
            IReadOnlyList<PrimaryConstructorParameter> buildableParameters)
        {
            var statements = new List<StatementSyntax>();

            statements.Add(GetFailuresListDeclaration("validationFailures"));

            foreach (var primaryConstructorField in buildableParameters)
            {
                statements.Add(
                    CheckPropertyForNull(
                        primaryConstructorField.MemberName,
                        "validationFailures"));
            }

            statements.Add(CheckForAnyValidationFailures("validationFailures"));

            statements.Add(
                ReturnBuildStatement(builderTargetTypeName, buildableParameters));

            var body = Block(List(statements));

            return MethodDeclaration(
                    IdentifierName(builderTargetTypeName),
                    Identifier("Build"))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithBody(body);
        }

        private static IEnumerable<MemberDeclarationSyntax> GetBuilderMembers(
            string builderTargetTypeName,
            IReadOnlyList<PrimaryConstructorParameter> buildableParameters)
        {
            foreach (var buildableParameter in buildableParameters)
            {
                yield return GetPropertyDeclaration(
                    buildableParameter.MemberName,
                    buildableParameter.Type);
            }

            yield return GenerateBuildMethod(builderTargetTypeName, buildableParameters);
        }

        private static LocalDeclarationStatementSyntax
            GetFailuresListDeclaration(string validationFailuresVariableName) =>
            LocalDeclarationStatement(
                VariableDeclaration(
                        IdentifierName(
                            Identifier(
                                TriviaList(),
                                SyntaxKind.VarKeyword,
                                "var",
                                "var",
                                TriviaList())))
                    .WithVariables(
                        SingletonSeparatedList(
                            VariableDeclarator(Identifier(validationFailuresVariableName))
                                .WithInitializer(
                                    EqualsValueClause(
                                        ObjectCreationExpression(
                                                QualifiedName(
                                                    QualifiedName(
                                                        QualifiedName(
                                                            IdentifierName("System"),
                                                            IdentifierName(
                                                                "Collections")),
                                                        IdentifierName("Generic")),
                                                    GenericName(Identifier("List"))
                                                        .WithTypeArgumentList(
                                                            TypeArgumentList(
                                                                SingletonSeparatedList<
                                                                    TypeSyntax>(
                                                                    PredefinedType(
                                                                        Token(
                                                                            SyntaxKind
                                                                                .StringKeyword)))))))
                                            .WithArgumentList(ArgumentList()))))));

        private static ReturnStatementSyntax ReturnBuildStatement(
            string targetTypeName,
            IReadOnlyList<PrimaryConstructorParameter> buildableParameters)
        {
            var syntaxNodeOrTokens = new List<SyntaxNodeOrToken>();

            var first = true;

            foreach (var buildableParameter in buildableParameters)
            {
                if (!first)
                {
                    syntaxNodeOrTokens.Add(Token(SyntaxKind.CommaToken));
                }

                syntaxNodeOrTokens.Add(
                    Argument(IdentifierName(buildableParameter.MemberName)));

                first = false;
            }

            return ReturnStatement(
                ObjectCreationExpression(IdentifierName(targetTypeName))
                    .WithArgumentList(
                        ArgumentList(SeparatedList<ArgumentSyntax>(syntaxNodeOrTokens))));
        }

        private static IfStatementSyntax
            CheckForAnyValidationFailures(string validationFailuresVariableName) =>
            IfStatement(
                BinaryExpression(
                    SyntaxKind.GreaterThanExpression,
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(validationFailuresVariableName),
                        IdentifierName("Count")),
                    LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0))),
                Block(
                    SingletonList<StatementSyntax>(
                        ThrowStatement(
                            ObjectCreationExpression(IdentifierName("BuilderException"))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(
                                                IdentifierName(
                                                    "validationFailures")))))))));

        private static IfStatementSyntax
            CheckPropertyForNull(
                string propertyName,
                string validationFailuresVariableName) =>
            IfStatement(
                IsPatternExpression(
                    IdentifierName(propertyName),
                    ConstantPattern(LiteralExpression(SyntaxKind.NullLiteralExpression))),
                Block(
                    SingletonList<StatementSyntax>(
                        ExpressionStatement(
                            InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(validationFailuresVariableName),
                                        IdentifierName("Add")))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(
                                                LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression,
                                                    Literal(
                                                        $"{propertyName} is null"))))))))));

        private static PropertyDeclarationSyntax
            GetPropertyDeclaration(string propertyName, TypeSyntax type) =>
            PropertyDeclaration(type, Identifier(propertyName))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));
    }

    public static class SyntaxExtensions
    {
        public static PropertyDeclarationSyntax WithAccessors(
            this PropertyDeclarationSyntax propertyDeclaration,
            params AccessorDeclarationSyntax[] accessors)
        {
            return propertyDeclaration.WithAccessorList(AccessorList(List(accessors)));
        }
    }
}
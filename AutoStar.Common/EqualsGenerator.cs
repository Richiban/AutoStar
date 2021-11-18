using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AutoStar.Common
{
    public static class EqualsGenerator
    {
        public static (MethodDeclarationSyntax objectEquals, MethodDeclarationSyntax
            equatableEquals) GenerateEqualsMethodForClass(
                ClassDeclarationSyntax classDeclaration)
        {
            var qualifyingMembers = GetQualifyingMembers(classDeclaration);

            return (GenerateObjectEqualsMethod(),
                GenerateEqualsMethodForMembers(
                    qualifyingMembers,
                    classDeclaration.Identifier));
        }

        private static MethodDeclarationSyntax GenerateObjectEqualsMethod() =>
            MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.BoolKeyword)),
                    Identifier("Equals"))
                .WithModifiers(
                    TokenList(
                        Token(SyntaxKind.PublicKeyword),
                        Token(SyntaxKind.OverrideKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(Identifier("other"))
                                .WithType(
                                    PredefinedType(Token(SyntaxKind.ObjectKeyword))))))
                .WithExpressionBody(
                    ArrowExpressionClause(
                        BinaryExpression(
                            SyntaxKind.LogicalAndExpression,
                            IsPatternExpression(
                                IdentifierName("other"),
                                DeclarationPattern(
                                    IdentifierName("TestClass"),
                                    SingleVariableDesignation(Identifier("otherClass")))),
                            InvocationExpression(IdentifierName("Equals"))
                                .WithArgumentList(
                                    ArgumentList(
                                        SingletonSeparatedList(
                                            Argument(IdentifierName("otherClass"))))))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));

        private static MethodDeclarationSyntax GenerateEqualsMethodForMembers(
            IEnumerable<MemberDeclarationSyntax> members,
            SyntaxToken classIdentifier)
        {
            var otherObjectName = "other";

            var equalsExpression = CompareToNull(otherObjectName);

            foreach (var member in members)
            {
                equalsExpression = AddEqualsExpression(
                    equalsExpression,
                    otherObjectName,
                    member);
            }

            const string equals = "Equals";

            return MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.BoolKeyword)),
                    Identifier(equals))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                .WithParameterList(
                    ParameterList(
                        SingletonSeparatedList(
                            Parameter(Identifier(otherObjectName))
                                .WithType(IdentifierName(classIdentifier)))))
                .WithBody(
                    Block(
                        SingletonList<StatementSyntax>(
                            ReturnStatement(equalsExpression))));
        }

        private static BinaryExpressionSyntax CompareToNull(string otherObjectName) =>
            BinaryExpression(
                SyntaxKind.NotEqualsExpression,
                IdentifierName(otherObjectName),
                LiteralExpression(SyntaxKind.NullLiteralExpression));

        private static BinaryExpressionSyntax AddEqualsExpression(
            BinaryExpressionSyntax equalsExpression,
            string otherObjectName,
            MemberDeclarationSyntax member) =>
            BinaryExpression(
                SyntaxKind.LogicalAndExpression,
                equalsExpression,
                GetMemberEqualsExpression(otherObjectName, GetMemberName(member)));

        private static string GetMemberName(MemberDeclarationSyntax member) =>
            member switch
            {
                FieldDeclarationSyntax fieldDeclaration => fieldDeclaration.Declaration
                    .Variables.Single()
                    .Identifier.Text,
                PropertyDeclarationSyntax propertyDeclaration => propertyDeclaration
                    .Identifier.Text,
                _ => throw new NotSupportedException($"Cannot get the name of {member}")
            };

        private static InvocationExpressionSyntax
            GetMemberEqualsExpression(string otherObjectName, string memberName) =>
            InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName(memberName)),
                        IdentifierName("Equals")))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList(
                            Argument(
                                MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName(otherObjectName),
                                    IdentifierName(memberName))))));

        private static IEnumerable<MemberDeclarationSyntax> GetQualifyingMembers(
            ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.Members.Where(
                it => it is FieldDeclarationSyntax ||
                      it is PropertyDeclarationSyntax p && IsAutoProperty(p));
        }

        private static bool IsAutoProperty(PropertyDeclarationSyntax propertyDeclaration)
        {
            return propertyDeclaration.AccessorList?.Accessors.All(
                it => it.Body is null) == true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.Common
{
    public class ToStringGenerator
    {
        public MemberDeclarationSyntax GenerateToStringMethodForClass(
            ClassDeclarationSyntax classDeclaration)
        {
            var qualifyingMembers = GetQualifyingMembers(classDeclaration);

            return GenerateToStringMethodForMembers(
                qualifyingMembers,
                classDeclaration.Identifier);
        }

        private MemberDeclarationSyntax GenerateToStringMethodForMembers(
            IEnumerable<MemberDeclarationSyntax> members,
            SyntaxToken classIdentifier)
        {
            return SyntaxFactory.MethodDeclaration(
                attributeLists: SyntaxFactory.List<AttributeListSyntax>(),
                modifiers: GetToStringModifiers(),
                returnType: SyntaxFactory.IdentifierName("string"),
                identifier: SyntaxFactory.Identifier("ToString"),
                parameterList: SyntaxFactory.ParameterList(),
                constraintClauses:
                SyntaxFactory.List<TypeParameterConstraintClauseSyntax>(),
                body: GetToStringBody(members, classIdentifier),
                typeParameterList: null,
                explicitInterfaceSpecifier: null,
                expressionBody: null);
        }

        private BlockSyntax GetToStringBody(
            IEnumerable<MemberDeclarationSyntax> members,
            SyntaxToken classIdentifier)
        {
            var builder = SyntaxFactory.IdentifierName("stringBuilder");

            var statements = new List<StatementSyntax>();

            AddStringBuilderVariableCreation(statements, builder);

            AddStringBuilderClassHeader(classIdentifier, statements, builder);

            var first = true;

            foreach (var member in members)
            {
                if (!first)
                {
                    AddMemberStringSeparator(statements, builder);
                }

                var memberName = GetMemberName(member);

                AddMemberStringHeader(statements, builder, memberName);
                AddMemberStringValue(statements, builder, memberName);

                first = false;
            }

            AddClosingBraceToStringBuilder(statements, builder);

            AddReturnStringBuilderToString(statements, builder);

            return SyntaxFactory.Block(statements);
        }

        private void AddReturnStringBuilderToString(
            List<StatementSyntax> statements,
            IdentifierNameSyntax builder)
        {
            statements.Add(
                SyntaxFactory.ReturnStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            builder,
                            SyntaxFactory.IdentifierName("ToString")))));
        }

        private void AddMemberStringSeparator(
            List<StatementSyntax> statements,
            IdentifierNameSyntax builder)
        {
            statements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            builder,
                            SyntaxFactory.IdentifierName("Append")),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal($", "))))))));
        }

        private static void AddStringBuilderClassHeader(
            SyntaxToken classIdentifier,
            List<StatementSyntax> statements,
            IdentifierNameSyntax builder)
        {
            statements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            builder,
                            SyntaxFactory.IdentifierName("Append")),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(
                                            $"{classIdentifier} {{ "))))))));
        }

        private static void AddStringBuilderVariableCreation(
            List<StatementSyntax> statements,
            IdentifierNameSyntax stringBuilderIdentifier)
        {
            statements.Add(
                SyntaxFactory.LocalDeclarationStatement(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.IdentifierName("StringBuilder"),
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.VariableDeclarator(
                                SyntaxFactory.Identifier(
                                    stringBuilderIdentifier.Identifier.Text),
                                argumentList: null,
                                SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory
                                        .ObjectCreationExpression(
                                            SyntaxFactory.IdentifierName("StringBuilder"))
                                        .WithArgumentList(
                                            SyntaxFactory.ArgumentList())))))));
        }

        private static void AddClosingBraceToStringBuilder(
            List<StatementSyntax> statements,
            IdentifierNameSyntax builder)
        {
            statements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            builder,
                            SyntaxFactory.IdentifierName("Append")),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal(" }"))))))));
        }

        private static SyntaxToken GetMemberName(MemberDeclarationSyntax? member)
        {
            return member switch
            {
                FieldDeclarationSyntax field => field.Declaration.Variables.First()
                    .Identifier,
                PropertyDeclarationSyntax prop => prop.Identifier,
                _ => throw new InvalidOperationException(
                    $"Could not get identifier from type {member}")
            };
        }

        private static void AddMemberStringValue(
            List<StatementSyntax> statements,
            IdentifierNameSyntax builder,
            SyntaxToken memberName)
        {
            statements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            builder,
                            SyntaxFactory.IdentifierName("Append")),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.ThisExpression(),
                                        SyntaxFactory.IdentifierName(memberName))))))));
        }

        private static void AddMemberStringHeader(
            List<StatementSyntax> statements,
            IdentifierNameSyntax builder,
            SyntaxToken memberName)
        {
            statements.Add(
                SyntaxFactory.ExpressionStatement(
                    SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            builder,
                            SyntaxFactory.IdentifierName("Append")),
                        SyntaxFactory.ArgumentList(
                            SyntaxFactory.SingletonSeparatedList(
                                SyntaxFactory.Argument(
                                    SyntaxFactory.LiteralExpression(
                                        SyntaxKind.StringLiteralExpression,
                                        SyntaxFactory.Literal($"{memberName} = "))))))));
        }

        private SyntaxTokenList GetToStringModifiers()
        {
            return SyntaxFactory.TokenList(
                SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                SyntaxFactory.Token(SyntaxKind.OverrideKeyword));
        }

        private IEnumerable<MemberDeclarationSyntax> GetQualifyingMembers(
            ClassDeclarationSyntax classDeclaration)
        {
            return classDeclaration.Members.Where(
                it => it is FieldDeclarationSyntax ||
                      it is PropertyDeclarationSyntax p && IsAutoProperty(p));
        }

        public bool IsAutoProperty(PropertyDeclarationSyntax propertyDeclaration)
        {
            return propertyDeclaration.AccessorList?.Accessors.All(
                it => it.Body is null) == true;
        }
    }
}
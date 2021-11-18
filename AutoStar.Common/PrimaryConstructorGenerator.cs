using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.Common
{
    public static class PrimaryConstructorGenerator
    {
        public static Option<ConstructorDeclarationSyntax> GetNewConstructor(
            ClassDeclarationSyntax classDeclaration)
        {
            var fields = GetFields(classDeclaration);

            if (!fields.Any())
            {
                return null;
            }
            
            var classDeclarationIdentifier = classDeclaration.Identifier;

            return GetNewConstructor(classDeclarationIdentifier, fields);
        }

        private static ConstructorDeclarationSyntax GetNewConstructor(
            SyntaxToken classIdentifier,
            ImmutableArray<PrimaryConstructorField> fields) =>
            SyntaxFactory
                .ConstructorDeclaration(classIdentifier)
                .WithBody(GetConstructorBody(fields))
                .WithParameterList(GetParameterList(fields))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

        private static ImmutableArray<PrimaryConstructorField> GetFields(
            ClassDeclarationSyntax classDeclarationSyntax)
        {
            return classDeclarationSyntax.ChildNodes()
                .OfType<FieldDeclarationSyntax>()
                .Where(f => f.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword)))
                .SelectMany(f => PrimaryConstructorField.From(f))
                .ToImmutableArray();
        }

        private static BlockSyntax GetConstructorBody(
            IEnumerable<PrimaryConstructorField> fields)
        {
            return SyntaxFactory.Block(
                SyntaxFactory.List(fields.Select(GetFieldAssignment)));
        }

        private static ParameterListSyntax GetParameterList(
            IEnumerable<PrimaryConstructorField> fields) =>
            SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(fields.Select(MapParameter)));

        private static ParameterSyntax MapParameter(PrimaryConstructorField field)
        {
            var identifier = SyntaxFactory.Identifier(field.ParameterName);

            return SyntaxFactory.Parameter(identifier)
                .WithType(SyntaxFactory.IdentifierName(field.TypeName));
        }

        private static ExpressionStatementSyntax GetFieldAssignment(
            PrimaryConstructorField field)
        {
            var fieldIdentifier = SyntaxFactory.IdentifierName(field.FieldName);

            var parameterIdentifier = SyntaxFactory.IdentifierName(field.ParameterName);

            var assignmentExpression = SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                fieldIdentifier,
                parameterIdentifier);

            return SyntaxFactory.ExpressionStatement(assignmentExpression);
        }
    }
}
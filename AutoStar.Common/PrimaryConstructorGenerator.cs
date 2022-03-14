using System;
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
            var fields = GetReadonlyFieldsAndProperties(classDeclaration);

            if (!fields.Any())
            {
                return null;
            }

            var classDeclarationIdentifier = classDeclaration.Identifier;

            return GetNewConstructor(classDeclarationIdentifier, fields);
        }

        private static ConstructorDeclarationSyntax GetNewConstructor(
            SyntaxToken classIdentifier,
            ImmutableArray<PrimaryConstructorParameter> fields) =>
            SyntaxFactory.ConstructorDeclaration(classIdentifier)
                .WithBody(GetConstructorBody(fields))
                .WithParameterList(GetParameterList(fields))
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword)));

        private static ImmutableArray<PrimaryConstructorParameter> GetReadonlyFieldsAndProperties(
            ClassDeclarationSyntax classDeclarationSyntax)
        {
            var parametersFromFields = classDeclarationSyntax.ChildNodes()
                .OfType<FieldDeclarationSyntax>()
                .Where(f => f.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword)))
                .SelectMany(PrimaryConstructorParameter.FromField);

            var parametersFromProperties = classDeclarationSyntax.ChildNodes()
                .OfType<PropertyDeclarationSyntax>()
                .Where(IsGetOnlyAutoProperty)
                .Select(PrimaryConstructorParameter.FromProperty);

            return parametersFromFields
                .Concat(parametersFromProperties)
                .ToImmutableArray();
        }

        private static bool IsGetOnlyAutoProperty(PropertyDeclarationSyntax p) =>
            p.AccessorList?.Accessors switch
            {
                { Count: 1 } accessors => accessors.All(it => it.Body is null),
                _ => false
            };

        private static BlockSyntax GetConstructorBody(
            IEnumerable<PrimaryConstructorParameter> fields) =>
            SyntaxFactory.Block(SyntaxFactory.List(fields.Select(GetFieldAssignment)));

        private static ParameterListSyntax GetParameterList(
            IEnumerable<PrimaryConstructorParameter> fields) =>
            SyntaxFactory.ParameterList(
                SyntaxFactory.SeparatedList(fields.Select(MapParameter)));

        private static ParameterSyntax MapParameter(PrimaryConstructorParameter parameter)
        {
            var identifier = SyntaxFactory.Identifier(parameter.ParameterName);

            return SyntaxFactory.Parameter(identifier).WithType(parameter.Type);
        }

        private static ExpressionStatementSyntax GetFieldAssignment(
            PrimaryConstructorParameter parameter)
        {
            var fieldIdentifier = SyntaxFactory.IdentifierName(parameter.MemberName);

            var parameterIdentifier = SyntaxFactory.IdentifierName(parameter.ParameterName);

            var assignmentExpression = SyntaxFactory.AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                fieldIdentifier,
                parameterIdentifier);

            return SyntaxFactory.ExpressionStatement(assignmentExpression);
        }
    }
}
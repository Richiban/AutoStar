using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.EnumClass
{
    public class EnumClassConverter
    {
        public static ClassDeclarationSyntax ConvertEnumClass(
            ClassDeclarationSyntax classDeclaration,
            SyntaxToken? outerClassIdentifier = null,
            bool makePublic = false)
        {
            var newModifiers = makePublic
                ? AddModifiers(
                    classDeclaration.Modifiers,
                    SyntaxKind.PublicKeyword,
                    SyntaxKind.AbstractKeyword)
                : AddModifiers(classDeclaration.Modifiers, SyntaxKind.AbstractKeyword);

            var classDeclarationSyntax = classDeclaration
                .WithMembers(GetNewMembers(classDeclaration))
                .WithAttributeLists(new SyntaxList<AttributeListSyntax>())
                .WithModifiers(newModifiers);

            if (outerClassIdentifier is { } outerClassIdentifierToken)
            {
                classDeclarationSyntax = classDeclarationSyntax.WithBaseList(
                    SyntaxFactory.BaseList(
                        SyntaxFactory.SeparatedList(
                            new[]
                            {
                                (BaseTypeSyntax)SyntaxFactory.SimpleBaseType(
                                    SyntaxFactory.IdentifierName(
                                        outerClassIdentifierToken))
                            })));
            }

            return classDeclarationSyntax;
        }

        private static SyntaxTokenList AddModifiers(
            SyntaxTokenList modifiers,
            params SyntaxKind[] syntaxKinds)
        {
            foreach (var xx in syntaxKinds)
            {
                modifiers = modifiers.Add(SyntaxFactory.Token(xx));
            }

            return SyntaxFactory.TokenList(modifiers.OrderBy(x => x.RawKind));
        }

        private static SyntaxList<MemberDeclarationSyntax> GetNewMembers(
            ClassDeclarationSyntax classDeclaration)
        {
            var memberDeclarations =
                new List<MemberDeclarationSyntax> { GetNewConstructor(classDeclaration) };

            memberDeclarations.AddRange(GetPartialInnerClasses(classDeclaration));

            return SyntaxFactory.List(memberDeclarations);
        }

        private static IEnumerable<MemberDeclarationSyntax> GetPartialInnerClasses(
            ClassDeclarationSyntax outerClassDeclaration) =>
            outerClassDeclaration.ChildNodes()
                .OfType<ClassDeclarationSyntax>()
                .Select(
                    decl => ConvertInnerClass(decl, outerClassDeclaration.Identifier));

        private static ClassDeclarationSyntax ConvertInnerClass(
            ClassDeclarationSyntax innerClassDeclaration,
            SyntaxToken outerClassIdentifier)
        {
            return IsEnumClass(innerClassDeclaration)
                ? ConvertEnumClass(
                    innerClassDeclaration,
                    outerClassIdentifier,
                    makePublic: true)
                : ConvertCaseClass(innerClassDeclaration, outerClassIdentifier);
        }

        private static bool IsEnumClass(ClassDeclarationSyntax innerClassDeclaration)
        {
            return innerClassDeclaration.AttributeLists.Any(
                x => x.Attributes.Any(y => y.Name.ToString() == "EnumClass"));
        }

        private static ClassDeclarationSyntax ConvertCaseClass(
            ClassDeclarationSyntax innerClassDeclaration,
            SyntaxToken outerClassName)
        {
            TypeSyntax typeSyntax = SyntaxFactory.IdentifierName(outerClassName);

            return innerClassDeclaration
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PublicKeyword),
                        SyntaxFactory.Token(SyntaxKind.SealedKeyword),
                        SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                .WithBaseList(
                    SyntaxFactory.BaseList(
                        SyntaxFactory.SeparatedList(
                            new[]
                            {
                                (BaseTypeSyntax)SyntaxFactory.SimpleBaseType(
                                    typeSyntax)
                            })));
        }

        private static ConstructorDeclarationSyntax
            GetNewConstructor(ClassDeclarationSyntax declaration) =>
            SyntaxFactory.ConstructorDeclaration(declaration.Identifier)
                .WithBody(GetConstructorBody())
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PrivateKeyword)));

        private static BlockSyntax GetConstructorBody() => SyntaxFactory.Block();
    }
}
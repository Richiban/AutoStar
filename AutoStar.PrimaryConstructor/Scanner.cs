using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoStar.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Win32.SafeHandles;

namespace AutoStar.PrimaryConstructor
{
    internal class Scanner
    {
        private readonly MarkerAttributeDefinition _attributeDefinition;
        private readonly Compilation _compilation;

        public Scanner(
            MarkerAttributeDefinition attributeDefinition,
            Compilation compilation)
        {
            _attributeDefinition = attributeDefinition;
            _compilation = compilation;
        }

        public ImmutableArray<ResultOption<ScanFailure, CompilationUnitSyntax>>
            BuildFrom(IEnumerable<ClassDeclarationSyntax> candidateClasses) =>
            candidateClasses.Select(Map).ToImmutableArray();

        private ResultOption<ScanFailure, CompilationUnitSyntax> Map(
            ClassDeclarationSyntax classDeclaration)
        {
            var model = _compilation.GetSemanticModel(classDeclaration.SyntaxTree);

            var classSymbol =
                ModelExtensions.GetDeclaredSymbol(model, classDeclaration) as
                    INamedTypeSymbol;

            if (classSymbol is null)
                return new ScanFailure(
                    ErrorId.Unknown,
                    $"Unknown problem with class {classDeclaration.Identifier}",
                    classDeclaration.GetLocation());

            if (!ClassHasCorrectAttribute(classSymbol))
                return new None();

            if (!ClassIsPartial(classDeclaration))
                return new ScanFailure(
                    ErrorId.MustBePartial,
                    $"Class {classDeclaration.Identifier} must be partial",
                    classDeclaration.GetLocation());

            if (ClassHasExistingConstructors(classDeclaration))
                return new ScanFailure(
                    ErrorId.MustNotHaveConstructors,
                    $"Class {classDeclaration.Identifier} must not define any constructors",
                    classDeclaration.GetLocation());

            var newConstructor =
                Common.PrimaryConstructorGenerator.GetNewConstructor(classDeclaration);

            return newConstructor.Select(
                c => GeneratePartialComplement(classDeclaration, c));
        }

        private CompilationUnitSyntax GeneratePartialComplement(
            ClassDeclarationSyntax classDeclaration,
            ConstructorDeclarationSyntax newConstructor)
        {
            var originalRoot = classDeclaration.SyntaxTree.HasCompilationUnitRoot
                ? classDeclaration.SyntaxTree.GetCompilationUnitRoot()
                : SyntaxFactory.CompilationUnit();

            var newClass = SyntaxFactory.ClassDeclaration(classDeclaration.Identifier)
                .WithModifiers(
                    SyntaxFactory.TokenList(
                        SyntaxFactory.Token(SyntaxKind.PartialKeyword)))
                .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>())
                .AddMembers(newConstructor);

            var namespaceDeclaration = originalRoot.Members
                .OfType<NamespaceDeclarationSyntax>()
                .SingleOrDefault(); // TODO : Handle multiple namespaces

            if (namespaceDeclaration != null)
            {
                var newNamespace = namespaceDeclaration
                    .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>())
                    .AddMembers(newClass);

                return originalRoot
                    .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>())
                    .AddMembers(newNamespace);
            }
            else
            {
                return originalRoot
                    .WithMembers(SyntaxFactory.List<MemberDeclarationSyntax>())
                    .AddMembers(newClass);
            }
        }

        private bool ClassIsPartial(ClassDeclarationSyntax declaration)
        {
            return declaration.Modifiers.Any(SyntaxKind.PartialKeyword);
        }

        private bool ClassHasCorrectAttribute(ISymbol classSymbol)
        {
            return classSymbol.GetAttributes().Any(_attributeDefinition.IsMatch);
        }

        private static bool ClassHasExistingConstructors(
            ClassDeclarationSyntax classSymbol)
        {
            return classSymbol.Members.OfType<ConstructorDeclarationSyntax>().Any();
        }

        private static string ToFullString(SyntaxNode newClassSyntax) =>
            newClassSyntax.SyntaxTree.GetRoot().NormalizeWhitespace().ToFullString();

        private static NamespaceDeclarationSyntax NamespaceWithNewClass(
            NamespaceDeclarationSyntax namespaceDeclaration,
            MemberDeclarationSyntax newClassSyntax,
            List<UsingDirectiveSyntax> usingDirectives) =>
            namespaceDeclaration.WithMembers(SyntaxFactory.List(new[] { newClassSyntax }))
                .WithUsings(SyntaxFactory.List(usingDirectives));

        private static NamespaceDeclarationSyntax? GetNamespace(SyntaxNode root) =>
            root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
        //
        // private static ClassDeclarationSyntax CreateNewClassSyntax() =>
        //     _classDeclaration.ClassDeclaration.WithMembers(GetNewMembers())
        //         .WithAttributeLists(new SyntaxList<AttributeListSyntax>());
        //
        // private static SyntaxList<MemberDeclarationSyntax> GetNewMembers()
        // {
        //     var newConstructor = Common.PrimaryConstructorGenerator.GetNewConstructor(
        //         _classDeclaration.ClassDeclaration);
        //
        //     return newConstructor.IsSome(out var constructor)
        //         ? SyntaxFactory.SingletonList((MemberDeclarationSyntax)constructor)
        //         : new();
        // }
    }
}
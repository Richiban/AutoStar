using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using AutoStar.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.EnumClass
{
    internal class Scanner
    {
        private readonly MarkerAttribute _attribute;
        private readonly Compilation _compilation;

        public Scanner(
            MarkerAttribute attribute,
            Compilation compilation)
        {
            _attribute = attribute;
            _compilation = compilation;
        }

        public ImmutableArray<ResultOption<ModelFailure, EnumClassModel>> BuildFor(
            IEnumerable<ClassDeclarationSyntax> candidateClasses) =>
            candidateClasses.Select(BuildFor).ToImmutableArray();

        private ResultOption<ModelFailure, EnumClassModel> BuildFor(
            ClassDeclarationSyntax classDeclaration)
        {
            var model = _compilation.GetSemanticModel(classDeclaration.SyntaxTree);

            if (ModelExtensions.GetDeclaredSymbol(model, classDeclaration) is not
                INamedTypeSymbol classSymbol)
            {
                return new ModelFailure(
                    ErrorId.Unknown,
                    ErrorMessages.Unknown,
                    classDeclaration.GetLocation());
            }

            if (!ClassHasCorrectAttribute(classSymbol))
            {
                return new ResultOption<ModelFailure, EnumClassModel>.None();
            }

            if (GetInnerClasses(classDeclaration) is not { Count: > 0 } innerClasses)
            {
                return new ResultOption<ModelFailure, EnumClassModel>.None();
            }

            if (!ClassIsPartial(classDeclaration))
            {
                return new ModelFailure(
                    ErrorId.MustBePartial,
                    ErrorMessages.MustBePartial(
                        classDeclaration.Identifier.ToString(),
                        _attribute.ShortName),
                    classDeclaration.GetLocation());
            }

            if (ClassHasExistingConstructors(classDeclaration))
            {
                return new ModelFailure(
                    ErrorId.MustNotHaveConstructors,
                    ErrorMessages.MustNotHaveConstructors(
                        classDeclaration.Identifier.ToString(),
                        _attribute.ShortName),
                    classDeclaration.GetLocation());
            }

            return new EnumClassModel(classSymbol.Name, classDeclaration, innerClasses);
        }

        private IReadOnlyList<ClassDeclarationSyntax> GetInnerClasses(
            ClassDeclarationSyntax classSymbol) =>
            classSymbol.ChildNodes().OfType<ClassDeclarationSyntax>().ToList();

        private bool ClassIsPartial(ClassDeclarationSyntax declaration) =>
            declaration.Modifiers.Any(SyntaxKind.PartialKeyword);

        private bool ClassHasCorrectAttribute(ISymbol classSymbol) =>
            classSymbol.GetAttributes().Any(_attribute.IsMatch);

        private static bool ClassHasExistingConstructors(
            ClassDeclarationSyntax classSymbol) =>
            classSymbol.Members.OfType<ConstructorDeclarationSyntax>().Any();
    }
}
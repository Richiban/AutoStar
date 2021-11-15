using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.EnumClass
{
    internal class ModelBuilder
    {
        private readonly EnumClassAttributeDefinition _attributeDefinition;
        private readonly Compilation _compilation;

        public ModelBuilder(
            EnumClassAttributeDefinition attributeDefinition,
            Compilation compilation)
        {
            _attributeDefinition = attributeDefinition;
            _compilation = compilation;
        }

        public ImmutableArray<MaybeResult<ModelFailure, EnumClassModel>> BuildFrom(
            IEnumerable<ClassDeclarationSyntax> candidateClasses) =>
            candidateClasses.Select(Map).ToImmutableArray();

        private MaybeResult<ModelFailure, EnumClassModel> Map(
            ClassDeclarationSyntax classDeclaration)
        {
            var model = _compilation.GetSemanticModel(classDeclaration.SyntaxTree);

            if (ModelExtensions.GetDeclaredSymbol(model, classDeclaration) is not
                INamedTypeSymbol classSymbol)
                return new ModelFailure(
                    ErrorId.Unknown,
                    ErrorMessages.Unknown,
                    classDeclaration.GetLocation());

            if (!ClassHasCorrectAttribute(classSymbol))
                return new MaybeResult<ModelFailure, EnumClassModel>.None();

            if (GetInnerClasses(classDeclaration) is not { Count: > 0 } innerClasses)
                return new MaybeResult<ModelFailure, EnumClassModel>.None();

            if (!ClassIsPartial(classDeclaration))
                return new ModelFailure(
                    ErrorId.MustBePartial,
                    ErrorMessages.MustBePartial(
                        classDeclaration.Identifier.ToString(),
                        _attributeDefinition.ShortName),
                    classDeclaration.GetLocation());

            if (ClassHasExistingConstructors(classDeclaration))
                return new ModelFailure(
                    ErrorId.MustNotHaveConstructors,
                    ErrorMessages.MustNotHaveConstructors(
                        classDeclaration.Identifier.ToString(),
                        _attributeDefinition.ShortName),
                    classDeclaration.GetLocation());

            return new EnumClassModel(classSymbol.Name, classDeclaration, innerClasses);
        }

        private IReadOnlyList<ClassDeclarationSyntax> GetInnerClasses(
            ClassDeclarationSyntax classSymbol) =>
            classSymbol.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();

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
    }
}
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.PrimaryConstructor
{
    internal class ModelBuilder
    {
        private readonly PrimaryConstructorAttributeDefinition _attributeDefinition;
        private readonly Compilation _compilation;

        public ModelBuilder(PrimaryConstructorAttributeDefinition attributeDefinition, Compilation compilation)
        {
            _attributeDefinition = attributeDefinition;
            _compilation = compilation;
        }

        public ImmutableArray<MaybeResult<ModelFailure, EnumClassModel>> BuildFrom(
            IEnumerable<ClassDeclarationSyntax> candidateClasses) =>
            candidateClasses.Select(Map).ToImmutableArray();

        private MaybeResult<ModelFailure, EnumClassModel> Map(ClassDeclarationSyntax classDeclaration)
        {
            var model = _compilation.GetSemanticModel(classDeclaration.SyntaxTree);

            var classSymbol = ModelExtensions.GetDeclaredSymbol(model, classDeclaration) as INamedTypeSymbol;

            if (classSymbol is null)
                return new ModelFailure(
                    ErrorId.Unknown,
                    $"Unknown problem with class {classDeclaration.Identifier}",
                    classDeclaration.GetLocation());

            if (!ClassHasCorrectAttribute(classSymbol))
                return new MaybeResult<ModelFailure, EnumClassModel>.None();

            if (GetReadonlyFields(classSymbol) is not {Count: > 0} fields)
                return new MaybeResult<ModelFailure, EnumClassModel>.None();

            if (!ClassIsPartial(classDeclaration))
                return new ModelFailure(
                    ErrorId.MustBePartial,
                    $"Class {classDeclaration.Identifier} must be partial", classDeclaration.GetLocation());

            if (ClassHasExistingConstructors(classDeclaration))
                return new ModelFailure(
                    ErrorId.MustNotHaveConstructors,
                    $"Class {classDeclaration.Identifier} must not define any constructors",
                    classDeclaration.GetLocation());

            return new EnumClassModel(classSymbol.Name, classDeclaration, fields);
        }

        private IReadOnlyList<FieldModel> GetReadonlyFields(INamedTypeSymbol classSymbol) => classSymbol.GetMembers()
            .OfType<IFieldSymbol>().Where(f => f.IsReadOnly).Select(f => new FieldModel(IdentifierName.Parse(f.Name), f.Type))
            .ToList();

        private bool ClassIsPartial(ClassDeclarationSyntax declaration)
        {
            return declaration.Modifiers.Any(SyntaxKind.PartialKeyword);
        }

        private bool ClassHasCorrectAttribute(ISymbol classSymbol)
        {
            return classSymbol.GetAttributes().Any(_attributeDefinition.IsMatch);
        }

        private static bool ClassHasExistingConstructors(ClassDeclarationSyntax classSymbol)
        {
            return classSymbol.Members.OfType<ConstructorDeclarationSyntax>().Any();
        }
    }
}
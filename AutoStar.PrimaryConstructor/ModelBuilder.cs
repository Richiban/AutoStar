using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.PrimaryConstructor
{
    class ModelBuilder
    {
        private readonly PrimaryConstructorAttributeDefinition _cmdrAttribute;
        private readonly Compilation _compilation;

        public ModelBuilder(PrimaryConstructorAttributeDefinition cmdrAttribute, Compilation compilation)
        {
            _cmdrAttribute = cmdrAttribute;
            _compilation = compilation;
        }

        public ImmutableArray<Result<ModelFailure, ClassModel>> BuildFrom(
            IEnumerable<ClassDeclarationSyntax> candidateClasses)
        {
            return candidateClasses.Select(Map).ToImmutableArray();
        }

        private Result<ModelFailure, ClassModel> Map(ClassDeclarationSyntax arg)
        {
            var model = _compilation.GetSemanticModel(arg.SyntaxTree);
            
            var classSymbol = model.GetDeclaredSymbol(arg);

            CheckForPresenceOfCorrectAttribute();

            CheckClassIsPartial();
            
            CheckThatAPublicConstructorIsNotAlreadyDeclared(classSymbol);

            return new ClassModel();
        }

        private static void CheckThatAPublicConstructorIsNotAlreadyDeclared(ISymbol? classSymbol)
        {
            var constructorSymbol = classSymbol.Constructors.FirstOrDefault(c => c.Parameters.Length == 0);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using AutoStar.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using CodeGenerationResult =
    AutoStar.Common.ResultOption<AutoStar.BuilderPattern.CodeGenerationFailure, System.
        Collections.Generic.IReadOnlyList<
            Microsoft.CodeAnalysis.CSharp.Syntax.ParameterSyntax>>;

namespace AutoStar.BuilderPattern
{
    class Scanner
    {
        private readonly MarkerAttribute _attribute;

        public Scanner(MarkerAttribute attribute, Compilation contextCompilation)
        {
            _attribute = attribute;
        }

        public IEnumerable<ResultOption<CodeGenerationFailure, CompilationUnitSyntax>>
            BuildFor(IEnumerable<ClassDeclarationSyntax> attributeMarkedClasses) =>
            attributeMarkedClasses.Select(BuildFor);

        public ResultOption<CodeGenerationFailure, CompilationUnitSyntax> BuildFor(
            ClassDeclarationSyntax attributeMarkedClass)
        {
            if (!ClassIsPartial(attributeMarkedClass))
            {
                return new CodeGenerationFailure(
                    ErrorId.MustBePartial,
                    ErrorMessages.MustBePartial(
                        attributeMarkedClass.Identifier.Text,
                        _attribute.ShortName),
                    attributeMarkedClass.GetLocation());
            }

            IReadOnlyList<PrimaryConstructorParameter> buildableParameters;

            switch (GetBuildableParameters(attributeMarkedClass))
            {
                case CodeGenerationResult.Err error:
                    return error.Convert<CompilationUnitSyntax>();

                case CodeGenerationResult.Ok(var constructorParameters):
                    buildableParameters = constructorParameters
                        .Select(x => PrimaryConstructorParameter.FromParameter(x))
                        .ToList();

                    break;

                case CodeGenerationResult.None:
                    if (PrimaryConstructorGenerator
                        .GetNewConstructor(attributeMarkedClass)
                        .IsSome(out var primaryConstructor))
                    {
                        buildableParameters = primaryConstructor.ParameterList.Parameters
                            .Select(x => PrimaryConstructorParameter.FromParameter(x))
                            .ToList();

                        break;
                    }

                    throw new InvalidOperationException(
                        "Got into an invalid state: tried to generate primary constructor but it failed");
                case var other:
                    throw new InvalidOperationException($"Unhandled {other}");
            }

            var newClass = GeneratePartialComplementTo(attributeMarkedClass)
                .WithMembers(List<MemberDeclarationSyntax>())
                .AddMembers(
                    BuilderClassGenerator.GenerateBuilderClass(
                        attributeMarkedClass.Identifier.Text,
                        buildableParameters),
                    BuilderExceptionGenerator.GenerateExceptionClass());

            var existingRoot = attributeMarkedClass.SyntaxTree.GetRoot();

            var existingUsings = existingRoot.DescendantNodes()
                .OfType<UsingDirectiveSyntax>()
                .ToList();

            var existingNamespaces = existingRoot.DescendantNodes()
                .OfType<NamespaceDeclarationSyntax>()
                .ToList();

            switch (existingNamespaces.Count)
            {
                case 0:
                    return CompilationUnit()
                        .WithUsings(List(existingUsings))
                        .WithMembers(SingletonList((MemberDeclarationSyntax)newClass));
                case 1:
                    return CompilationUnit()
                        .WithUsings(List(existingUsings))
                        .WithMembers(
                            SingletonList(
                                (MemberDeclarationSyntax)existingNamespaces.First()
                                    .WithMembers(
                                        SingletonList(
                                            (MemberDeclarationSyntax)newClass))));
                default:
                    throw new NotSupportedException(
                        "Multiple namespaces currently not supported");
            }
        }

        private bool ClassIsPartial(ClassDeclarationSyntax attributeMarkedClass)
        {
            return attributeMarkedClass.Modifiers.Any(
                modifier => modifier.IsKind(SyntaxKind.PartialKeyword));
        }

        private ClassDeclarationSyntax GeneratePartialComplementTo(
            ClassDeclarationSyntax attributeMarkedClass)
        {
            return attributeMarkedClass.WithMembers(List<MemberDeclarationSyntax>())
                .WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)))
                .WithAttributeLists(List<AttributeListSyntax>());
        }

        private CodeGenerationResult GetBuildableParameters(
            ClassDeclarationSyntax attributeMarkedClass)
        {
            var constructors = attributeMarkedClass.Members
                .OfType<ConstructorDeclarationSyntax>()
                .ToList();

            return constructors.Count switch
            {
                0 => default(None),
                1 => constructors.Single().ParameterList.Parameters,
                _ => new CodeGenerationFailure(
                    ErrorId.MustHaveExactlyOneConstructor,
                    ErrorMessages.MustHaveExactlyOneConstructor(
                        attributeMarkedClass.Identifier.Text,
                        _attribute.ShortName),
                    attributeMarkedClass.GetLocation())
            };
        }
    }
}
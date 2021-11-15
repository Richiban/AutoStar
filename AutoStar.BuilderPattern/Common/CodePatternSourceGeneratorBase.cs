using System;
using System.Collections.Generic;
using System.Linq;
using AutoStar.BuilderPattern.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.BuilderPattern.Common
{
    public abstract class CodePatternSourceGeneratorBase : ISourceGenerator
    {
        protected abstract string GeneratorName { get; }

        public void Execute(GeneratorExecutionContext context)
        {
            var compilation = InjectAttribute(context);

            var attributeSymbol = compilation.GetTypeByMetadataName(AttributeToInject.TypeDeclaration.Name)!;

            foreach (var syntaxTree in context.Compilation.SyntaxTrees)
            {
                var root = syntaxTree.GetRoot();
                var usings = (root as CompilationUnitSyntax)!.Usings.ToString();

                foreach (var classDeclaration in GetCandidateClasses(root, context.Compilation, attributeSymbol))
                {
                    var namespaceName = classDeclaration.ContainingNamespace.ToDisplayString();

                    GeneratePatternFor(classDeclaration, usings, namespaceName).AddTo(context);
                }
            }
        }

        private IEnumerable<INamedTypeSymbol> GetCandidateClasses(SyntaxNode root, Compilation compilation, INamedTypeSymbol attributeSymbol)
        {
            var candidateClasses =  root.DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                ;

            foreach (var @class in candidateClasses)
            {
                var model = compilation.GetSemanticModel(@class.SyntaxTree);
                var classSymbol = model.GetDeclaredSymbol(@class) as INamedTypeSymbol;

                //if (classSymbol.GetAttributes().Any(ad => ad.AttributeClass.Equals(attributeSymbol, SymbolEqualityComparer.Default)))
                if (classSymbol.GetAttributes().Any(ad => ad.AttributeClass.MetadataName.Equals(attributeSymbol.MetadataName)))
                {
                    yield return classSymbol;
                }
            }
        }

        private Compilation InjectAttribute(GeneratorExecutionContext context)
        {
            var attributeToInject = new GeneratedFile(AttributeToInject, GeneratorName);

            return attributeToInject.AddTo(context);
        }

        public virtual void Initialize(GeneratorInitializationContext context)
        {
            //if (!Debugger.IsAttached)
            //{
            //    Debugger.Launch();
            //}
        }

        public virtual TypeFile AttributeToInject =>
            new TypeFile("using System;\r\n",
                new ClassDeclaration(GeneratorName + "Attribute")
                {
                    Attribute = new AttributeA("AttributeUsage")
                    {
                        Arguments = new[] { "AttributeTargets.Class", "Inherited = true", "AllowMultiple = false" }
                    },
                    BaseClass = "Attribute"
                })
            {
                //NamespaceName = nameof(AutoStar)
            };

        public abstract GeneratedFile GeneratePatternFor(INamedTypeSymbol classSymbol, string usings, string @namespace);
    }
}
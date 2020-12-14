using AutoStar.Common;
using AutoStar.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics;
using System.Linq;

namespace AutoStar.PrimaryConstructor
{
    [Generator]
    public class PrimaryConstructorGenerator : CodePatternSourceGeneratorBase
    {
        protected override string GeneratorName => nameof(PrimaryConstructor);

        private static string ToCamelCase(string name)
        {
            name = name.TrimStart('_');

            return name.Substring(0, 1).ToLowerInvariant() + name.Substring(1);
        }

        public override GeneratedFile GeneratePatternFor(INamedTypeSymbol classSymbol, string usings, string @namespace)
        {
            var fieldList = classSymbol.GetMembers().OfType<IFieldSymbol>()
                .Where(x => x.CanBeReferencedByName && x.IsReadOnly)
                .Select(it => new { Type = it.Type.ToDisplayString(), ParameterName = ToCamelCase(it.Name), it.Name })
                .ToList();

            var parameters = fieldList.Select(it => new Parameter(it.ParameterName, it.Type)).ToList();
            var assignments = fieldList.Select(field => new AssignmentStatement($"this.{field.Name}", field.ParameterName)).ToList();
            var className = classSymbol.Name;

            var typeFile = new TypeFile(usings, new ClassDeclaration(className)
            {
                IsPartial = true,
                Constructor = new Constructor.BlockConstructor(className)
                {
                    Parameters = parameters,
                    Statements = assignments
                }
            })
            {
                NamespaceName = @namespace
            };

            return new GeneratedFile(typeFile, GeneratorName);
        }
    }
}
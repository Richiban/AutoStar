using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;
using AutoStar.BuilderPattern.InputModels;
using AutoStar.BuilderPattern.Model;

namespace AutoStar.BuilderPattern
{
    public class RecordConverter
    {
        public ClassDeclaration LowerRecordToClass(RecordDeclaration record)
        {
            var constructorParams = record.RecordProperties
                .Select(p => new Parameter(p.Name, p.Type))
                .ToArray();

            var constructorAssignments = record.RecordProperties
                .Select(p => new AssignmentStatement($"this.{p.Name}", p.Name))
                .ToArray();

            var validateMethod = new MethodDeclaration(
                "Validate",
                "void")
            {
                IsStatic = false,
                IsOverride = false,
                Visibility = Visibility.Public,
                Contents =
                    new IWriteableCode[] { new Statement("var errors = new System.Collections.Generic.List<string>();") }
                        .Concat(record.RecordProperties
                            .Where(p => p.IsOptional == false)
                            .Select(p => new Statement($"if ({p.Name} is null) errors.Add(\"{p.Name} is null\");")))
                        .Concat(new IWriteableCode[] { new Statement(
                            "if (errors.Count > 0) throw new ValidationException(errors);") })
                        .ToArray()
            };

            var buildMethod = new MethodDeclaration(
                "Build",
                record.Name)
            {
                Visibility = Visibility.Public,
                IsStatic = false,
                IsOverride = false,
                Contents = new IWriteableCode[] {
                    new Statement("Validate();"),
                    new ConstructorCall(record.Name, record.RecordProperties.Select(p => p.Name).ToArray()) }};

            var builderException = BuildBuilderExceptionDefinition();

            var toStringMethod = BuildToStringMethod(record);

            var builderProps = record.RecordProperties
                .Select(p => new Property(p.Name, p.Type) { HasSetter = true, DefaultValue = p.DefaultValue })
                .Cast<IWriteableCode>()
                .Concat(new IWriteableCode[] { validateMethod, buildMethod, builderException })
                .ToArray();

            var builderClass = new ClassDeclaration("Builder")
            {
                Visibility = Visibility.Public,
                Contents = builderProps
            };

            var output = new ClassDeclaration(record.Name)
            {
                Constructor = new Constructor.BlockConstructor(record.Name) 
                { 
                    Visibility = Visibility.Public, 
                    Parameters = constructorParams, 
                    Statements = constructorAssignments
                },
                Visibility = Visibility.Public,
                IsPartial = true,
                Contents = new IWriteableCode[] { toStringMethod, builderClass }
            };

            return output;
        }

        private MethodDeclaration BuildToStringMethod(RecordDeclaration record)
        {
            var propNames = String.Join($",{Environment.NewLine}", record.RecordProperties.Select(p => $"$\"{p.Name} = {{{p.Name}}}\""));

            var statements = new IWriteableCode[] {
                new Statement($"var elements = new string[] {{ {propNames} }};"),
                new Statement($"var s = System.String.Join(\", \", elements);"),
                new Statement($"return $\"{record.Name} {{{{ {{s}} }}}}\";")
            };

            return new MethodDeclaration("ToString", "string")
            {
                Visibility = Visibility.Public,
                IsStatic = false,
                IsOverride = true,
                Contents = statements
            };
        }

        private static ClassDeclaration BuildBuilderExceptionDefinition()
        {
            return new ClassDeclaration("ValidationException") 
            {
                Visibility = Visibility.Public,
                Contents = new IWriteableCode[] {
                    new Property("Errors", "System.Collections.Generic.IReadOnlyCollection<string>")
                    {
                        HasSetter = false,
                        Visibility = Visibility.Public
                    },
                    new MethodDeclaration("GetMessage", "string")
                    {
                        Visibility = Visibility.Private,
                        IsStatic =  true,
                        IsOverride =  false,
                        Parameters =  new Parameter[] {new Parameter("errors", "System.Collections.Generic.IReadOnlyCollection<string>") },
                        Contents =  new IWriteableCode[] { new Statement("return string.Join(System.Environment.NewLine, errors);")}
                    }
                },
                BaseClass =  "System.Exception",
                Constructor =  new Constructor.BlockConstructor("ValidationException") {
                    Visibility = Visibility.Public,
                    Parameters = new[] { new Parameter("errors", "System.Collections.Generic.IReadOnlyCollection<string>") },
                    BaseCall = new[] { "GetMessage(errors)" },
                    Statements = new[] { new AssignmentStatement("Errors", "errors") }
                }
            };
        }

        public RecordDeclaration ClassToRecord(ClassDeclarationSyntax classDeclaration)
        {
            var className = classDeclaration.Identifier.Text;

            var props = classDeclaration.Members
                .OfType<PropertyDeclarationSyntax>()
                .Select(CreateProp)
                .ToList();

            return new RecordDeclaration(className, props);
        }

        private RecordProperty CreateProp(PropertyDeclarationSyntax prop)
        {
            var isOptional = prop.Type is NullableTypeSyntax;
            var defaultValue = prop.Initializer?.ToString();

            return new RecordProperty(
                name: prop.Identifier.ToString(),
                type: prop.Type.ToFullString(),
                isOptional: isOptional,
                defaultValue: defaultValue);
        }
    }
}
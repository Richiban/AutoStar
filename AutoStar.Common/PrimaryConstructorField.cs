using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.Common
{
    public class PrimaryConstructorParameter
    {
        private PrimaryConstructorParameter(
            string parameterName,
            string memberName,
            TypeSyntax type)
        {
            ParameterName = parameterName;
            MemberName = memberName;
            Type = type;
        }

        public string ParameterName { get; }
        public string MemberName { get; }
        public TypeSyntax Type { get; }

        public static IEnumerable<PrimaryConstructorParameter> FromField(
            FieldDeclarationSyntax fieldDeclarationSyntax)
        {
            return fieldDeclarationSyntax.Declaration.Variables.Select(
                v => new PrimaryConstructorParameter(
                    IdentifierNaming.Create(v.Identifier.ValueText).ToCamelCase(),
                    v.Identifier.ValueText,
                    fieldDeclarationSyntax.Declaration.Type));
        }

        public static PrimaryConstructorParameter FromParameter(
            ParameterSyntax parameterSyntax)
        {
            var name = parameterSyntax.Identifier.ValueText;

            return new PrimaryConstructorParameter(
                name,
                IdentifierNaming.Create(name).ToPascalCase(),
                parameterSyntax.Type!);
        }

        public static PrimaryConstructorParameter FromProperty(
            PropertyDeclarationSyntax propertySyntax)
        {
            var name = propertySyntax.Identifier.ValueText;

            return new(
                name,
                IdentifierNaming.Create(name).ToPascalCase(),
                propertySyntax.Type!);
        }
    }
}
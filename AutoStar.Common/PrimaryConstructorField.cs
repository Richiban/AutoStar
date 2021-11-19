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

        public static PrimaryConstructorParameter FromParameter(ParameterSyntax parameterSyntax) =>
            new PrimaryConstructorParameter(
                parameterSyntax.Identifier.ValueText,
                IdentifierNaming.Create(parameterSyntax.Identifier.ValueText).ToPascalCase(),
                parameterSyntax.Type!);
    }
}
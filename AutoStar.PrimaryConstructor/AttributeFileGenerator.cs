namespace AutoStar.PrimaryConstructor
{
    internal class AttributeFileGenerator
    {
        private readonly PrimaryConstructorAttributeDefinition _attributeDefinition;

        public AttributeFileGenerator(PrimaryConstructorAttributeDefinition attributeDefinition)
        {
            _attributeDefinition = attributeDefinition;
        }

        public string FileName => "PrimaryConstructorAttribute.g.cs";

        public string GetCode()
        {
            return @"using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PrimaryConstructorAttribute : Attribute
{}";
        }
    }
}
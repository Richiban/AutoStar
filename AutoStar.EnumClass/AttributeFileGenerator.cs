namespace AutoStar.EnumClass
{
    internal class AttributeFileGenerator
    {
        private readonly EnumClassAttributeDefinition _attributeDefinition;

        public AttributeFileGenerator(EnumClassAttributeDefinition attributeDefinition)
        {
            _attributeDefinition = attributeDefinition;
        }

        public string FileName => _attributeDefinition.LongName + ".cs";

        public string GetCode()
        {
            return @"using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class EnumClassAttribute : Attribute
{}";
        }
    }
}
using System;

namespace AutoStar.BuilderPattern.InputModels
{
    public class RecordProperty
    {
        public RecordProperty(
            string name,
            string type,
            bool isOptional = false,
            string? defaultValue = null)
        {
            Name = name;
            Type = type;
            IsOptional = isOptional;
            DefaultValue = defaultValue;
        }

        public string Name { get; }
        public string Type { get; }
        public bool IsOptional { get; }
        public string? DefaultValue { get; }
    }
}
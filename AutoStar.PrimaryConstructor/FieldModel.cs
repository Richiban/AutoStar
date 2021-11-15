using Microsoft.CodeAnalysis;

namespace AutoStar.PrimaryConstructor
{
    public class FieldModel
    {
        public FieldModel(IdentifierName name, ITypeSymbol type)
        {
            Name = name;
            Type = type;
        }

        public IdentifierName Name { get; }
        public ITypeSymbol Type { get; }
    }
}
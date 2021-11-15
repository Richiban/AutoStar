using System;

namespace AutoStar.BuilderPattern.Model
{
    public interface ITypeDeclaration : IWriteableCode
    {
        string Name { get; }
    }
}
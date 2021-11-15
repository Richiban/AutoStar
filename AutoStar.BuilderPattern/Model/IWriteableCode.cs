using System;

namespace AutoStar.BuilderPattern.Model
{
    public interface IWriteableCode
    {
        void WriteTo(CodeBuilder codeBuilder);
    }
}
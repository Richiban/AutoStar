using System;

namespace AutoStar.Common
{
    public interface ICodeFileGenerator
    {
        string FileName { get; }
        string GetCode();
    }
}
using System;

namespace AutoStar.Common
{
    public interface ICodeFileGenerator
    {
        string GetCode();
        string FileName { get; }
    }
}
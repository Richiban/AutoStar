namespace AutoStar.PrimaryConstructor
{
    internal abstract class CodeFileGenerator
    {
        public abstract string FileName { get; }
        public abstract string GetCode();
    }
}
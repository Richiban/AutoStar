namespace AutoStar.PrimaryConstructor
{
    internal abstract class CodeFileGenerator
    {
        protected CodeFileGenerator(string fileName)
        {
            FileName = fileName;
        }

        public abstract string GetCode();
        public string FileName { get; }
    }
}
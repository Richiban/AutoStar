namespace AutoStar.PrimaryConstructor
{
    class PrimaryConstructorClassFileGenerator : CodeFileGenerator
    {
        public PrimaryConstructorClassFileGenerator(ClassModel model)
        {
            FileName = model.ClassName + ".g.cs";
        }

        public override string FileName { get; }
        public override string GetCode()
        {
            return ""; // TODO
        }
    }
}
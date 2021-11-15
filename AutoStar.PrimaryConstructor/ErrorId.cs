namespace AutoStar.PrimaryConstructor
{
    class ErrorId
    {
        private readonly string _value;

        private ErrorId(string value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return _value;
        }

        public static ErrorId MustBePartial { get; } = new("ASPC0001");
        public static ErrorId MustNotHaveConstructors { get; } = new("ASPC0002");
        public static ErrorId Unknown { get; } = new("ASPC0000");
    }
}
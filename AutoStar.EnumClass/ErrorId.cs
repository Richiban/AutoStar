namespace AutoStar.EnumClass
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

        public static ErrorId MustBePartial { get; } = new("ASEC0001");
        public static ErrorId MustNotHaveConstructors { get; } = new("ASEC0002");
        public static ErrorId Unknown { get; } = new("ASEC0000");
    }
}
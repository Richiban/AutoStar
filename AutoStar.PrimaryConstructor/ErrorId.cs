using System;

namespace AutoStar.PrimaryConstructor
{
    internal class ErrorId
    {
        private readonly string _value;

        private ErrorId(string value)
        {
            _value = value;
        }

        public static ErrorId MustBePartial { get; } = new("ASPC0001");
        public static ErrorId MustNotHaveConstructors { get; } = new("ASPC0002");
        public static ErrorId Unknown { get; } = new("ASPC0000");

        public override string ToString() => _value;
    }
}
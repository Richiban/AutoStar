using System;

namespace AutoStar.BuilderPattern
{
    internal class ErrorId
    {
        private readonly string _value;

        private ErrorId(string value)
        {
            _value = value;
        }

        public static ErrorId MustBePartial { get; } = new("ASBP0001");
        public static ErrorId MustHaveExactlyOneConstructor { get; } = new("ASBP0002");
        public static ErrorId Unknown { get; } = new("ASBP0000");

        public override string ToString() => _value;
    }
}
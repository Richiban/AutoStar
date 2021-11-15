using Microsoft.CodeAnalysis;

namespace AutoStar.PrimaryConstructor
{
    internal class ModelFailure
    {
        public ModelFailure(ErrorId errorId, string message, Location? location)
        {
            Message = message;
            Location = location;
            ErrorId = errorId;
        }

        public ErrorId ErrorId { get; }
        public string Message { get; }
        public Location? Location { get; }
    }

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

    static class ErrorMessages
    {
        public static string Unknown => "There was an unknown error";

        public static string MustBePartial(string typeName)
        {
            return $"{typeName} must be partial";
        }

        public static string MustNotHaveConstructors(string typeName)
        {
            return $"{typeName} must not have constructors";
        }
    }
}
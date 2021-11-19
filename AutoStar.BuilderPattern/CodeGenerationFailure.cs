using Microsoft.CodeAnalysis;

namespace AutoStar.BuilderPattern
{
    class CodeGenerationFailure
    {
        public CodeGenerationFailure(ErrorId errorId, string message, Location? location)
        {
            ErrorId = errorId;
            Message = message;
            Location = location;
        }

        public ErrorId ErrorId { get; }
        public string Message { get; }
        public Location? Location { get; }
    }
}
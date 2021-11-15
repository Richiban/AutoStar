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
}
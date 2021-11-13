using Microsoft.CodeAnalysis;

namespace AutoStar.PrimaryConstructor
{
    internal class ModelFailure
    {
        public ModelFailure(string message, Location? location)
        {
            Message = message;
            Location = location;
        }

        public string Message { get; }
        public Location? Location { get; }
    }
}
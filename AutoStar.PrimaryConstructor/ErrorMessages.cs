using System;

namespace AutoStar.PrimaryConstructor
{
    internal static class ErrorMessages
    {
        public static string Unknown => "There was an unknown error";

        public static string MustBePartial(string typeName) =>
            $"{typeName} must be partial";

        public static string MustNotHaveConstructors(string typeName) =>
            $"{typeName} must not have constructors";
    }
}
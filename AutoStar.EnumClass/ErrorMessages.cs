namespace AutoStar.PrimaryConstructor
{
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
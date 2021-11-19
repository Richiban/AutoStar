using System;

namespace AutoStar.BuilderPattern
{
    internal static class ErrorMessages
    {
        public static string Unknown => "There was an unknown error";

        public static string MustBePartial(string typeName, string attributeName) =>
            $"The class {typeName} must be partial in order to use the {attributeName} attribute";

        public static string
            MustHaveExactlyOneConstructor(string typeName, string attributeName) =>
            $"Class {typeName} must define exactly one constructor to use the {attributeName} attribute";
    }
}
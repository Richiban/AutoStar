﻿using System;

namespace AutoStar.EnumClass
{
    internal static class ErrorMessages
    {
        public static string Unknown => "There was an unknown error";

        public static string MustBePartial(string typeName, string attributeName) =>
            $"The class {typeName} must be partial in order to use the {attributeName} attribute";

        public static string
            MustNotHaveConstructors(string typeName, string attributeName) =>
            $"The class {typeName} must not define any constructors in order to use the {attributeName} attribute";
    }
}
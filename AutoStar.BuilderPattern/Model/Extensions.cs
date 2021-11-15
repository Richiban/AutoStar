using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoStar.BuilderPattern.Model
{
    public static class Extensions
    {
        public static void WriteAll(this CodeBuilder codeBuilder, IReadOnlyCollection<IWriteableCode> items, string joiner = "")
        {
            var isFirst = true;

            foreach (var p in items)
            {
                if (isFirst == false)
                {
                    codeBuilder.Append(joiner);
                }

                p.WriteTo(codeBuilder);
                isFirst = false;
            }
        }

        //public static bool HasAttribute(this SyntaxList<AttributeListSyntax> attributes, string name)
        //{
        //    string fullname, shortname;
        //    var attrLen = "Attribute".Length;
        //    if (name.EndsWith("Attribute"))

        //    {
        //        fullname = name;
        //        shortname = name.Remove(name.Length - attrLen, attrLen);
        //    }
        //    else
        //    {
        //        fullname = name + "Attribute";
        //        shortname = name;
        //    }

        //    return attributes.Any(al => al.Attributes.Any(a => a.Name.ToString() == shortname || a.Name.ToString() == fullname));
        //}

        //public static T? FindParent<T>(this SyntaxNode node) where T : class
        //{
        //    var current = node;
        //    while (true)
        //    {
        //        current = current.Parent;
        //        if (current == null || current is T)
        //            return current as T;
        //    }
        //}
    }
}
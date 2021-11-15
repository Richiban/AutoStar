using System;
using System.Linq;

namespace AutoStar.BuilderPattern.Model
{
    public class Visibility : IWriteableCode
    {
        private string _value;
        private Visibility(string value) { _value = value; }

        public void WriteTo(CodeBuilder sb) => sb.Append(_value);

        public static Visibility Public { get; } = new Visibility("public ");
        public static Visibility Private { get; } = new Visibility("private ");
        public static Visibility Internal { get; } = new Visibility("internal ");
        public static Visibility None { get; } = new Visibility("");
    }
}
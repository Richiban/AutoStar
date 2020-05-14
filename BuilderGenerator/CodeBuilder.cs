﻿using System;
using System.Linq;
using System.Text;

namespace BuilderGenerator
{
    public class CodeBuilder
    {
        private readonly StringBuilder _sb = new StringBuilder();
        private int IndentationLevel = 0;

        public void AppendLine(string s)
        {
            A();
            _sb.AppendLine(s);
            WriteIndentation = true;
        }

        public void Append(string s)
        {
            A();

            _sb.Append(s);
            WriteIndentation = false;
        }

        public override string ToString()
        {
            return _sb.ToString();
        }

        internal void IncreaseIndentation()
        {
            IndentationLevel++;
        }

        internal void DecreaseIndentation()
        {
            IndentationLevel = Math.Max(0, IndentationLevel - 1);
        }

        public IDisposable Indent()
        {
            return new CodeBuilderIndentor(this);
        }

        private void A()
        {
            if (WriteIndentation)
                foreach (var _ in Enumerable.Range(0, IndentationLevel * 4))
                {
                    _sb.Append(' ');
                }

            WriteIndentation = false;
        }

        private bool WriteIndentation = true;

        private class CodeBuilderIndentor : IDisposable
        {
            private readonly CodeBuilder _cb;

            public CodeBuilderIndentor(CodeBuilder cb)
            {
                _cb = cb;
                _cb.IncreaseIndentation();
            }

            public void Dispose() => _cb.DecreaseIndentation();
        }
    }
}
using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AutoStar.Common
{
    public abstract class ResultOption<TError, TSuccess>
    {
        private ResultOption()
        {
        }

        public class Ok : ResultOption<TError, TSuccess>
        {
            public TSuccess Value { get; }

            public Ok(TSuccess value)
            {
                Value = value;
            }

            public void Deconstruct(out TSuccess value)
            {
                value = Value;
            }
        }

        public class Err : ResultOption<TError, TSuccess>
        {
            public TError Error { get; }

            public Err(TError error)
            {
                Error = error;
            }

            public void Deconstruct(out TError error)
            {
                error = Error;
            }
        }

        public class None : ResultOption<TError, TSuccess>
        {
        }

        public static implicit operator ResultOption<TError, TSuccess>(TSuccess value)
        {
            return new Ok(value);
        }

        public static implicit operator ResultOption<TError, TSuccess>(TError error)
        {
            return new Err(error);
        }

        public static implicit operator ResultOption<TError, TSuccess>(
            Option<TSuccess> option) =>
            From(option);

        public static ResultOption<TError, TSuccess> From(Option<TSuccess> option)
        {
            if (option.IsSome(out var value))
            {
                return new Ok(value);
            }

            return new None();
        }
        
        public static implicit operator ResultOption<TError, TSuccess>(Common.None _) =>
            new None();
    }
}
using System;

namespace AutoStar.Common
{
    public readonly struct Option<T>
    {
        private readonly T? _value;

        private Option(T? value)
        {
            _value = value;
        }

        public bool HasValue => _value != null;

        public static implicit operator Option<T>(T? value) => new(value);

        public bool IsSome(out T value)
        {
            value = _value!;

            return HasValue;
        }

        public Option<TResult> Select<TResult>(Func<T, TResult> func)
        {
            if (_value != null)
            {
                return func(_value);
            }

            return default;
        }
    }
}
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
        
        public static implicit operator Option<T>(None _) => default;

        public Result<TError, T> ToResult<TError>(Func<TError> mapNoneToError)
        {
            if (_value is not null)
            {
                return new Result<TError, T>.Ok(_value);
            }
            
            return new Result<TError, T>.Error(mapNoneToError());
        }
        
        public Result<TError, T> ToResult<TError>(TError error)
        {
            if (_value is not null)
            {
                return new Result<TError, T>.Ok(_value);
            }
            
            return new Result<TError, T>.Error(error);
        }
    }
}
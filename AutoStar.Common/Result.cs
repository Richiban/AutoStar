﻿using System;

namespace AutoStar.Common
{
    public abstract class Result<TError, TSuccess>
    {
        private Result()
        {
        }

        public static implicit operator Result<TError, TSuccess>(TSuccess success) =>
            new Ok(success);

        public static implicit operator Result<TError, TSuccess>(TError error) =>
            new Error(error);

        public sealed class Ok : Result<TError, TSuccess>
        {
            private readonly TSuccess _value;

            public Ok(TSuccess value)
            {
                _value = value;
            }

            public void Deconstruct(out TSuccess value) => value = _value;
        }

        public sealed class Error : Result<TError, TSuccess>
        {
            private readonly TError _error;

            public Error(TError error)
            {
                _error = error;
            }

            public void Deconstruct(out TError error) => error = _error;
        }

        public Result<TError, TResult> MapSuccess<TResult>(Func<TSuccess, TResult> func) =>
            this switch
            {
                Ok(var value) => new Result<TError, TResult>.Ok(func(value)),
                Error(var error) => new Result<TError, TResult>.Error(error),
                _ => throw new ArgumentOutOfRangeException()
            };
    }
}
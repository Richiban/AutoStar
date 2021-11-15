namespace AutoStar.PrimaryConstructor
{
    abstract class MaybeResult<TError, TSuccess>
    {
        private MaybeResult()
        {
        }

        public class Ok : MaybeResult<TError, TSuccess>
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

        public class Err : MaybeResult<TError, TSuccess>
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

        public class None : MaybeResult<TError, TSuccess>
        {
        }
        
        public static implicit operator MaybeResult<TError, TSuccess>(TSuccess value)
        {
            return new Ok(value);
        }
        
        public static implicit operator MaybeResult<TError, TSuccess>(TError error)
        {
            return new Err(error);
        }
    }

    internal abstract class Result<TError, TSuccess>
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
    }
}
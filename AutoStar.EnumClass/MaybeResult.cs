namespace AutoStar.EnumClass
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
}
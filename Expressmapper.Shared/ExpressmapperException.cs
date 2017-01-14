using System;

namespace ExpressMapper
{
    public class ExpressmapperException : Exception
    {
        public ExpressmapperException()
        {
        }

        public ExpressmapperException(string message)
            : base(message)
        {
        }

        public ExpressmapperException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}

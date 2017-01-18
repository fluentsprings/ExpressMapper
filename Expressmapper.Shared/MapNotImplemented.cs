using System;

namespace ExpressMapper
{
    /// <summary>
    /// Mapping not implemented exception
    /// </summary>
    /// Serializable turned off - PCL support
    //[Serializable]
    public class MapNotImplementedException : Exception
    {
        public MapNotImplementedException()
        {
        }
        public MapNotImplementedException(string message)
            : base(message)
        {
        }

        public MapNotImplementedException(string message, Exception inner)
            : base(message, inner)
        {
        }

        // Serializable turned off - PCL support
        //protected MapNotImplementedException(SerializationInfo info, StreamingContext context)
        //    : base(info, context)
        //{
        //}
    }
}

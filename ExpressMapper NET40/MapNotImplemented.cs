using System;

namespace ExpressMapper
{
    /// <summary>
    /// Not implemented exception
    /// </summary>
    public class MapNotImplemented : Exception
    {
        public MapNotImplemented(Type src, Type dest, string message) : base(message)
        {
        }

        public Type SourceType { get; set; }
        public Type DestinationType { get; set; }

    }
}

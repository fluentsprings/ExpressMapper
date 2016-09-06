using System;

namespace ExpressMapper
{
    public interface ICustomTypeMapper
    {
    }

    /// <summary>
    /// Interface to implement custom mapperd
    /// </summary>
    public interface ICustomTypeMapper<T, TN> : ICustomTypeMapper
    {
        TN Map(IMappingContext<T, TN> context);
    }
}

using System;
using System.Collections.Generic;

namespace ExpressMapper
{
    internal interface IMappingServiceProvider
    {
        void Compile();
        void PrecompileCollection<T, TN>();
        TN Map<T, TN>(T src, TN dest = default(TN));
        object Map(Type srcType, Type dstType, object src, object dest = null);
        IMemberConfiguration<T, TN> Register<T, TN>();
        void RegisterCustom<T, TN, TMapper>() where TMapper : ICustomTypeMapper<T, TN>;
        void RegisterCustom<T, TN>(Func<T, TN> mapFunc);
        void Reset();
        int CalculateCacheKey(Type src, Type dest);
        Dictionary<int, Func<ICustomTypeMapper>> CustomMappers { get; }
    }
}

using System;

namespace ExpressMapper
{
    public interface IMappingService
    {
        void Compile();
        TN Map<T, TN>(T src);
        TN Map<T, TN>(T src, TN dest);
        TN Map<TN>(object src);
        object Map(object src, Type srcType, Type dstType);
        object Map(object src, object dest, Type srcType, Type dstType);
        IMemberConfiguration<T, TN> Register<T, TN>();
        void RegisterCustom<T, TN, TMapper>() where TMapper : ICustomTypeMapper<T, TN>;
        void RegisterCustom<T, TN>(Func<T, TN> mapFunc);
        void Reset();
    }
}

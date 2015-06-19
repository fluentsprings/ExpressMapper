using System;

namespace ExpressMapper
{
    public interface IMappingService
    {
        void Compile();
        TN Map<T, TN>(T src);
        IMemberConfiguration<T, TN> Register<T, TN>();
        void RegisterCustom<T, TN, TMapper>() where TMapper : ICustomTypeMapper<T, TN>;
        void RegisterCustom<T, TN>(Func<T, TN> mapFunc);
        void Reset();
    }
}

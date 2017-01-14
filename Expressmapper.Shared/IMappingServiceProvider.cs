using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpressMapper
{
    public interface IMappingServiceProvider
    {
        void Compile();
        void Compile(CompilationTypes compilationType);
        void PrecompileCollection<T, TN>();
        void PrecompileCollection<T, TN>(CompilationTypes compilationType);
        TN Map<T, TN>(T src);
        TN Map<T, TN>(T src, TN dest);
        TN Map<T, TN>(T src, ICustomTypeMapper<T, TN> customMapper);
        TN Map<T, TN>(T src, ICustomTypeMapper<T, TN> customMapper, TN dest);
        object Map(Type srcType, Type dstType, object src);
        object Map(Type srcType, Type dstType, object src, object dest);
        IMemberConfiguration<T, TN> Register<T, TN>();
        IMemberConfiguration<T, TN> Register<T, TN>(IMemberConfigParameters baseType);
        bool MapExists(Type sourceType, Type destinationType);
        void RegisterCustom<T, TN, TMapper>() where TMapper : ICustomTypeMapper<T, TN>;
        void RegisterCustom<T, TN>(Func<T, TN> mapFunc);
        void Reset();
        long CalculateCacheKey(Type src, Type dest);
        Dictionary<long, Func<ICustomTypeMapper>> CustomMappers { get; }
        Dictionary<int, IList<long>> CustomMappingsBySource { get; }
        IQueryable<TN> Project<T, TN>(IQueryable<T> source);
        bool CaseSensetiveMemberMap { get; set; }
    }
}

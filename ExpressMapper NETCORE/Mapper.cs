using System;
using System.Linq;

namespace ExpressMapper
{
    public static class Mapper
    {
        private static IMappingServiceProvider _instance;

        // todo: via Internal DependencyResolver 
        public static IMappingServiceProvider Instance
        {
            get { return _instance ?? (_instance = new MappingServiceProvider()); }
        }

        public static void Compile()
        {
            Instance.Compile();
        }

        public static void Compile(CompilationTypes compilationType)
        {
            Instance.Compile(compilationType);
        }

        public static void PrecompileCollection<T,TN>()
        {
            Instance.PrecompileCollection<T, TN>();
        }

        public static void PrecompileCollection<T, TN>(CompilationTypes compilationType)
        {
            Instance.PrecompileCollection<T, TN>(compilationType);
        }

        public static TN Map<T, TN>(T src)
        {
            return Instance.Map<T, TN>(src);
        }

        public static TN Map<T, TN>(T src, TN dest)
        {
            return Instance.Map(src, dest);
        }

        internal static IQueryable<TN> Project<T, TN>(IQueryable<T> source)
        {
            return Instance.Project<T, TN>(source);
        }

        public static TN Map<T, TN>(T src, ICustomTypeMapper<T, TN> customMapper, TN dest = default(TN))
        {
            return Instance.Map(src, customMapper, dest);
        }

        public static object Map(object src, Type srcType, Type dstType)
        {
            return Instance.Map(srcType, dstType, src);
        }

        public static object Map(object src, object dest, Type srcType, Type dstType)
        {
            return Instance.Map(srcType, dstType, src, dest);
        }

        public static IMemberConfiguration<T, TN> Register<T, TN>()
        {
            return Instance.Register<T, TN>();
        }

        public static bool MapExists(Type sourceType, Type destinationType)
        {
            return Instance.MapExists(sourceType, destinationType);
        }

        public static void RegisterCustom<T, TN, TMapper>()
            where TMapper : ICustomTypeMapper<T, TN>
        {
            Instance.RegisterCustom<T, TN, TMapper>();
        }

        public static void RegisterCustom<T, TN>(Func<T, TN> mapFunc)
        {
            Instance.RegisterCustom<T, TN>(mapFunc);
        }

        public static void Reset()
        {
            Instance.Reset();
        }

        public static void MemberCaseSensitiveMap(bool caseSensitive)
        {
            Instance.CaseSensetiveMemberMap = caseSensitive;
        }
    }
}

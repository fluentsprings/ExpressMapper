using System;

namespace ExpressMapper
{
    public static class Mapper
    {
        private static readonly object _lock = new object();
        private static IMappingServiceProvider _instance;

        // todo: via Internal DependencyResolver 
        private static IMappingServiceProvider Instance
        {
            get { return _instance ?? (_instance = new MappingServiceProvider()); }
        }

        public static void Compile()
        {
            lock (_lock)
            {
                Instance.Compile();
            }
        }

        public static void PrecompileCollection<T,TN>()
        {
            lock (_lock)
            {
                Instance.PrecompileCollection<T, TN>();
            }
        }

        public static TN Map<T, TN>(T src)
        {
            return Instance.Map<T, TN>(src);
        }

        public static TN Map<T, TN>(T src, TN dest)
        {
            return Instance.Map(src, dest);
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
            lock (_lock)
            {
                return Instance.Register<T, TN>();
            }
        }

        public static void RegisterCustom<T, TN, TMapper>()
            where TMapper : ICustomTypeMapper<T, TN>
        {
            lock (_lock)
            {
                Instance.RegisterCustom<T, TN, TMapper>();
            }
        }

        public static void RegisterCustom<T, TN>(Func<T, TN> mapFunc)
        {
            lock (_lock)
            {
                Instance.RegisterCustom<T, TN>(mapFunc);
            }
        }

        public static void Reset()
        {
            lock (_lock)
            {
                Instance.Reset();
            }
        }
    }
}

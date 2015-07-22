using System;

namespace ExpressMapper
{
    public static class Mapper
    {
        private static IMappingService _instance;

        public static IMappingService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MappingService();
                }

                return _instance;
            }
        }

        public static void Compile()
        {
            Instance.Compile();
        }

        public static void PrecompileCollection<T,TN>()
        {
            Instance.PrecompileCollection<T, TN>();
        }

        public static TN Map<T, TN>(T src)
        {
            return Instance.Map<T, TN>(src);
        }

        public static TN Map<T, TN>(T src, TN dest)
        {
            return Instance.Map<T, TN>(src, dest);
        }

        public static object Map(object src, Type srcType, Type dstType)
        {
            return Instance.Map(src, srcType, dstType);
        }

        public static object Map(object src, object dest, Type srcType, Type dstType)
        {
            return Instance.Map(src, dest, srcType, dstType);
        }

        public static IMemberConfiguration<T, TN> Register<T, TN>()
        {
            return Instance.Register<T, TN>();
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
    }
}

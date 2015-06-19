using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressMapper
{
    public static class Mapper
    {
        private readonly static Dictionary<int, ITypeMapper> TypeMappers = new Dictionary<int, ITypeMapper>();
        private readonly static Dictionary<int, Func<ICustomTypeMapper>> CustomMappers = new Dictionary<int, Func<ICustomTypeMapper>>();
        private readonly static Dictionary<int, MulticastDelegate> CustomSimpleMappers = new Dictionary<int, MulticastDelegate>();

        public static IMemberConfiguration<T,TN> Register<T, TN>()
        {
            var classMapper = new TypeMapper<T, TN>();
            var cacheKey = CalculateCacheKey(typeof(T), typeof(TN));
            TypeMappers[cacheKey] = classMapper;
            return new MemberConfiguration<T, TN>(classMapper);
        }

        internal static List<Expression> GetMapExpressions(Type src, Type dest)
        {
            var cacheKey = CalculateCacheKey(src, dest);
            if (TypeMappers.ContainsKey(cacheKey))
            {
                return TypeMappers[cacheKey].GetMapExpressions();
            }
            throw new MapNotImplemented(src, dest, string.Format("There is no mapping has bee found. Source Type: {0}, Destination Type: {1}", src.FullName, dest.FullName));
        }

        public static void Compile()
        {
            foreach (var typeMapper in TypeMappers)
            {
                typeMapper.Value.Compile();
            }
        }

        public static void Reset()
        {
            TypeMappers.Clear();
        }

        public static void RegisterCustom<T, TN>(Func<T, TN> mapFunc)
        {
            var cacheKey = CalculateCacheKey(typeof(T), typeof(TN));
            CustomSimpleMappers.Add(cacheKey, mapFunc);
        }

        public static void RegisterCustom<T, TN, TMapper>() where TMapper : ICustomTypeMapper<T, TN>
        {
            var cacheKey = CalculateCacheKey(typeof(T), typeof(TN));

            var newExpression = Expression.New(typeof(TMapper));
            var newLambda = Expression.Lambda<Func<ICustomTypeMapper<T, TN>>>(newExpression);
            var compile = newLambda.Compile();
            CustomMappers.Add(cacheKey, compile);
        }

        public static TN Map<T, TN>(T src)
        {

            var cacheKey = CalculateCacheKey(typeof(T), typeof(TN));

            if (CustomMappers.ContainsKey(cacheKey))
            {
                var customTypeMapper = CustomMappers[cacheKey];
                var typeMapper = customTypeMapper() as ICustomTypeMapper<T,TN>;
                return typeMapper.Map(src);
            }

            if (CustomSimpleMappers.ContainsKey(cacheKey))
            {
                var funcMap = CustomSimpleMappers[cacheKey] as Func<T, TN>;
                return funcMap(src);
            }

            if (TypeMappers.ContainsKey(cacheKey))
            {
                if (EqualityComparer<T>.Default.Equals(src, default(T)))
                {
                    return default(TN);
                }

                var mapper = TypeMappers[cacheKey] as ITypeMapper<T, TN>;
                return mapper != null ? mapper.MapTo(src) : default(TN);
            }

            var colType = CollectionTypes.None;
            Type tnCol;

            var tCol =
                typeof(T).GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                (typeof(T).IsGenericType
                    && typeof(T).GetInterfaces().Any(t => t == typeof(IEnumerable)) ? typeof(T)
                    : null);

            tnCol = typeof(TN).GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IQueryable<>)) ??
                        (typeof(TN).IsGenericType && typeof(TN).GetInterfaces().Any(t => t == typeof(IQueryable)) ? typeof(TN)
                            : null);
            if (tnCol != null)
            {
                colType = CollectionTypes.Queryable;
            }

            if (colType == CollectionTypes.None)
            {
                tnCol = typeof(TN).GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                        (typeof(TN).IsGenericType && typeof(TN).GetInterfaces().Any(t => t == typeof(IEnumerable)) ? typeof(TN)
                            : null);
                colType = typeof(TN).IsArray ? CollectionTypes.Array : colType;
            }

            if (tCol != null && tnCol != null)
            {
                if (EqualityComparer<T>.Default.Equals(src, default(T)))
                {
                    return default(TN);
                }
                var sourceType = tCol.GetGenericArguments()[0];
                var destType = tnCol.GetGenericArguments()[0];
                var calculateCacheKey = CalculateCacheKey(sourceType, destType);
                if (TypeMappers.ContainsKey(calculateCacheKey))
                {
                    switch (colType)
                    {
                            case CollectionTypes.Queryable:
                            return (TN)TypeMappers[calculateCacheKey].ProcessQueryable(src as IQueryable);

                            case CollectionTypes.Array:
                            return (TN)TypeMappers[calculateCacheKey].ProcessArray(src as IEnumerable);

                            default:
                            return (TN)TypeMappers[calculateCacheKey].ProcessCollection(src as IEnumerable);
                    }
                }
            }
            throw new MapNotImplemented(typeof(T), typeof(TN), string.Format("There is no mapping has bee found. Source Type: {0}, Destination Type: {1}", typeof(T).FullName, typeof(TN).FullName));
        }

        private static int CalculateCacheKey(Type source, Type dest)
        {
            var srcHash = source.GetHashCode();
            return srcHash + dest.GetHashCode() / srcHash;
        }
    }
}

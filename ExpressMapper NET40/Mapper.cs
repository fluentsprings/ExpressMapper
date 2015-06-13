using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressMapper
{
    public static class Mapper
    {
        public static bool IsDestinationInstance = false;

        private readonly static Dictionary<int, ITypeMapper> TypeMappers = new Dictionary<int, ITypeMapper>();
        private readonly static Dictionary<int, Func<ICustomTypeMapper>> CustomMappers = new Dictionary<int, Func<ICustomTypeMapper>>();
        private readonly static Dictionary<int, MulticastDelegate> CustomSimpleMappers = new Dictionary<int, MulticastDelegate>();
        private readonly static Dictionary<int, MulticastDelegate> CustomSimpleMappersWithDest = new Dictionary<int, MulticastDelegate>();

        private readonly static Dictionary<int, Func<object, object>> CustomTypeMapperCache = new Dictionary<int, Func<object, object>>();
        private readonly static Dictionary<int, Func<object, object, object>> CustomTypeMapperWithDestCache = new Dictionary<int, Func<object, object, object>>();

        private readonly static Dictionary<int, BlockExpression> CustomTypeMapperExpCache = new Dictionary<int, BlockExpression>();
        private readonly static Dictionary<int, BlockExpression> CustomTypeMapperWithDestExpCache = new Dictionary<int, BlockExpression>();


        public static IMemberConfiguration<T,TN> Register<T, TN>()
        {
            var classMapper = new TypeMapper<T, TN>();
            var cacheKey = CalculateCacheKey(typeof(T), typeof(TN));
            TypeMappers[cacheKey] = classMapper;
            return new MemberConfiguration<T, TN>(classMapper);
        }

        internal static List<Expression> GetMapExpressions(Type src, Type dest, bool withDestinationInstance = false)
        {
            var cacheKey = CalculateCacheKey(src, dest);
            if (TypeMappers.ContainsKey(cacheKey))
            {
                return TypeMappers[cacheKey].GetMapExpressions(withDestinationInstance);
            }
            throw new MapNotImplemented(src, dest, string.Format("There is no mapping has bee found. Source Type: {0}, Destination Type: {1}", src.FullName, dest.FullName));
        }

        public static void Compile()
        {
            foreach (var typeMapper in TypeMappers)
            {
                typeMapper.Value.Compile();
                typeMapper.Value.CompileDestinationInstance();
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
                var defaultMappingContext = new DefaultMappingContext<T, TN> {Source = src};
                return typeMapper.Map(defaultMappingContext);
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

        public static TN Map<T, TN>(T src, TN dest)
        {
            var cacheKey = CalculateCacheKey(typeof(T), typeof(TN));

            if (CustomMappers.ContainsKey(cacheKey))
            {
                var customTypeMapper = CustomMappers[cacheKey];
                var typeMapper = customTypeMapper() as ICustomTypeMapper<T, TN>;
                var defaultMappingContext = new DefaultMappingContext<T, TN>{Source = src, Destination = dest};
                return typeMapper.Map(defaultMappingContext);
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
                return mapper != null ? mapper.MapTo(src, dest) : default(TN);
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
                //if (TypeMappers.ContainsKey(calculateCacheKey))
                //{
                //    switch (colType)
                //    {
                //        case CollectionTypes.Queryable:
                //            return (TN)TypeMappers[calculateCacheKey].ProcessQueryable(src as IQueryable, dest as IQueryable);

                //        case CollectionTypes.Array:
                //            return (TN)TypeMappers[calculateCacheKey].ProcessArray(src as IEnumerable, dest as IEnumerable);

                //        default:
                //            return (TN)TypeMappers[calculateCacheKey].ProcessCollection(src as IEnumerable, dest as IEnumerable);
                //    }
                //}
            }
            throw new MapNotImplemented(typeof(T), typeof(TN), string.Format("There is no mapping has bee found. Source Type: {0}, Destination Type: {1}", typeof(T).FullName, typeof(TN).FullName));
        }

        public static object Map(object src, Type srcType, Type dstType)
        {
            var cacheKey = CalculateCacheKey(srcType, dstType);

            if (CustomMappers.ContainsKey(cacheKey))
            {
                var customTypeMapper = CustomMappers[cacheKey];

                var typeMapper = customTypeMapper();

                if (!CustomTypeMapperCache.ContainsKey(cacheKey))
                {
                    CompileNonGenericCustomTypeMapper(srcType, dstType, typeMapper, cacheKey);
                }
                return CustomTypeMapperCache[cacheKey](src);
            }

            if (CustomSimpleMappers.ContainsKey(cacheKey))
            {
                var funcMap = CustomSimpleMappers[cacheKey];
                var result = funcMap.DynamicInvoke(src);
                return result;
            }

            if (TypeMappers.ContainsKey(cacheKey))
            {
                if (src == null)
                {
                    return null;
                }

                var mapper = TypeMappers[cacheKey];
                var nonGenericMapFunc = mapper.GetNonGenericMapFunc();

                return nonGenericMapFunc(src);
            }

            var colType = CollectionTypes.None;
            Type tnCol;

            var tCol =
                srcType.GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                (srcType.IsGenericType
                    && srcType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? srcType
                    : null);

            tnCol = dstType.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IQueryable<>)) ??
                        (dstType.IsGenericType && dstType.GetInterfaces().Any(t => t == typeof(IQueryable)) ? dstType
                            : null);
            if (tnCol != null)
            {
                colType = CollectionTypes.Queryable;
            }

            if (colType == CollectionTypes.None)
            {
                tnCol = dstType.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                        (dstType.IsGenericType && dstType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? dstType
                            : null);
                colType = dstType.IsArray ? CollectionTypes.Array : colType;
            }

            if (tCol != null && tnCol != null)
            {
                if (src == null)
                {
                    return null;
                }
                var sourceType = tCol.GetGenericArguments()[0];
                var destType = tnCol.GetGenericArguments()[0];
                var calculateCacheKey = CalculateCacheKey(sourceType, destType);
                if (TypeMappers.ContainsKey(calculateCacheKey))
                {
                    var typeMapper = TypeMappers[calculateCacheKey];
                    switch (colType)
                    {
                        case CollectionTypes.Queryable:
                            return typeMapper.ProcessQueryable(src as IQueryable);

                        case CollectionTypes.Array:
                            return typeMapper.ProcessArray(src as IEnumerable);

                        default:
                            return typeMapper.ProcessCollection(src as IEnumerable);
                    }
                }
            }
            throw new MapNotImplemented(srcType, dstType, string.Format("There is no mapping has bee found. Source Type: {0}, Destination Type: {1}", srcType.FullName, dstType.FullName));
        }

        public static object Map(object src, object dest, Type srcType, Type dstType)
        {
            var cacheKey = CalculateCacheKey(srcType, dstType);

            if (CustomMappers.ContainsKey(cacheKey))
            {
                var customTypeMapper = CustomMappers[cacheKey];

                var typeMapper = customTypeMapper();

                if (!CustomTypeMapperWithDestCache.ContainsKey(cacheKey))
                {
                    CompileNonGenericCustomTypeMapperWithDestination(srcType, dstType, typeMapper, cacheKey);
                }
                return CustomTypeMapperWithDestCache[cacheKey](src, dest);
            }

            if (CustomSimpleMappersWithDest.ContainsKey(cacheKey))
            {
                var funcMap = CustomSimpleMappersWithDest[cacheKey];
                var result = funcMap.DynamicInvoke(src, dest);
                return result;
            }

            if (TypeMappers.ContainsKey(cacheKey))
            {
                if (src == null)
                {
                    return null;
                }

                var mapper = TypeMappers[cacheKey];
                var nonGenericMapFunc = mapper.GetNonGenericMapFunc();

                return nonGenericMapFunc(src);
            }

            var colType = CollectionTypes.None;
            Type tnCol;

            var tCol =
                srcType.GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                (srcType.IsGenericType
                    && srcType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? srcType
                    : null);

            tnCol = dstType.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IQueryable<>)) ??
                        (dstType.IsGenericType && dstType.GetInterfaces().Any(t => t == typeof(IQueryable)) ? dstType
                            : null);
            if (tnCol != null)
            {
                colType = CollectionTypes.Queryable;
            }

            if (colType == CollectionTypes.None)
            {
                tnCol = dstType.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                        (dstType.IsGenericType && dstType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? dstType
                            : null);
                colType = dstType.IsArray ? CollectionTypes.Array : colType;
            }

            if (tCol != null && tnCol != null)
            {
                if (src == null)
                {
                    return null;
                }
                var sourceType = tCol.GetGenericArguments()[0];
                var destType = tnCol.GetGenericArguments()[0];
                var calculateCacheKey = CalculateCacheKey(sourceType, destType);
                if (TypeMappers.ContainsKey(calculateCacheKey))
                {
                    var typeMapper = TypeMappers[calculateCacheKey];
                    switch (colType)
                    {
                        case CollectionTypes.Queryable:
                            return typeMapper.ProcessQueryable(src as IQueryable);

                        case CollectionTypes.Array:
                            return typeMapper.ProcessArray(src as IEnumerable);

                        default:
                            return typeMapper.ProcessCollection(src as IEnumerable);
                    }
                }
            }
            throw new MapNotImplemented(srcType, dstType, string.Format("There is no mapping has bee found. Source Type: {0}, Destination Type: {1}", srcType.FullName, dstType.FullName));
        }

        #region Helper methods

        private static void CompileNonGenericCustomTypeMapper(Type srcType, Type dstType, ICustomTypeMapper typeMapper, int cacheKey)
        {
            var parameterExpression = Expression.Parameter(typeof (object), "src");
            var srcConverted = Expression.Convert(parameterExpression, srcType);
            var srcTypedExp = Expression.Variable(srcType, "srcTyped");
            var srcAssigned = Expression.Assign(srcTypedExp, srcConverted);

            var customGenericType = typeof (ICustomTypeMapper<,>).MakeGenericType(srcType, dstType);
            var castToCustomGeneric = Expression.Convert(Expression.Constant(typeMapper, typeof (ICustomTypeMapper)),
                customGenericType);
            var genVariable = Expression.Variable(customGenericType);
            var assignExp = Expression.Assign(genVariable, castToCustomGeneric);
            var methodInfo = customGenericType.GetMethod("Map");
            var genericMappingContext = typeof(DefaultMappingContext<,>).MakeGenericType(srcType, dstType);
            var newMappingContextExp = Expression.New(genericMappingContext);
            var sourceExp = Expression.Property(newMappingContextExp, "Source");
            var sourceAssignedExp = Expression.Assign(sourceExp, srcTypedExp);

            var mapCall = Expression.Call(genVariable, methodInfo, newMappingContextExp);
            var resultVarExp = Expression.Variable(typeof(object), "result");
            var resultAssignExp = Expression.Assign(resultVarExp, Expression.Convert(mapCall, typeof(object)));

            var blockExpression = Expression.Block(new[] {srcTypedExp, genVariable, resultVarExp},
                new Expression[] {srcAssigned, assignExp, sourceAssignedExp, resultAssignExp, resultVarExp});

            var lambda = Expression.Lambda<Func<object, object>>(blockExpression, parameterExpression);
            CustomTypeMapperCache.Add(cacheKey, lambda.Compile());
        }

        private static void CompileNonGenericCustomTypeMapperWithDestination(Type srcType, Type dstType, ICustomTypeMapper typeMapper, int cacheKey)
        {
            var sourceExpression = Expression.Parameter(typeof(object), "src");
            var destinationExpression = Expression.Parameter(typeof(object), "dest");
            var srcConverted = Expression.Convert(sourceExpression, srcType);
            var srcTypedExp = Expression.Variable(srcType, "srcTyped");
            var srcAssigned = Expression.Assign(srcTypedExp, srcConverted);
            
            var dstConverted = Expression.Convert(destinationExpression, dstType);
            var dstTypedExp = Expression.Variable(dstType, "destTyped");
            var dstAssigned = Expression.Assign(dstTypedExp, dstConverted);

            var customGenericType = typeof(ICustomTypeMapper<,>).MakeGenericType(srcType, dstType);
            var castToCustomGeneric = Expression.Convert(Expression.Constant(typeMapper, typeof(ICustomTypeMapper)),
                customGenericType);
            var genVariable = Expression.Variable(customGenericType);
            var assignExp = Expression.Assign(genVariable, castToCustomGeneric);
            var methodInfo = customGenericType.GetMethod("Map");
            var genericMappingContext = typeof(DefaultMappingContext<,>).MakeGenericType(srcType, dstType);
            var newMappingContextExp = Expression.New(genericMappingContext);
            var sourceExp = Expression.Property(newMappingContextExp, "Source");
            var sourceAssignedExp = Expression.Assign(sourceExp, srcTypedExp);
            var destinationAssignedExp = Expression.Assign(destinationExpression, dstTypedExp);

            var mapCall = Expression.Call(genVariable, methodInfo, newMappingContextExp);
            var resultVarExp = Expression.Variable(typeof(object), "result");
            var resultAssignExp = Expression.Assign(resultVarExp, Expression.Convert(mapCall, typeof(object)));

            var blockExpression = Expression.Block(new[] { srcTypedExp, genVariable, resultVarExp },
                new Expression[] { assignExp, sourceAssignedExp,destinationAssignedExp, resultAssignExp, resultVarExp });

            var lambda = Expression.Lambda<Func<object, object, object>>(blockExpression, sourceExpression, destinationExpression);
            CustomTypeMapperWithDestCache.Add(cacheKey, lambda.Compile());
        }

        private static void CompileGenericCustomTypeMapper(Type srcType, Type dstType, ICustomTypeMapper typeMapper, int cacheKey)
        {
            if (!CustomTypeMapperExpCache.ContainsKey(cacheKey))
            {
                var srcTypedExp = Expression.Variable(srcType, "srcTyped");

                var customGenericType = typeof (ICustomTypeMapper<,>).MakeGenericType(srcType, dstType);
                var castToCustomGeneric = Expression.Convert(
                    Expression.Constant(typeMapper, typeof (ICustomTypeMapper)),
                    customGenericType);
                var genVariable = Expression.Variable(customGenericType);
                var assignExp = Expression.Assign(genVariable, castToCustomGeneric);
                var methodInfo = customGenericType.GetMethod("Map");
                var genericMappingContext = typeof (DefaultMappingContext<,>).MakeGenericType(srcType, dstType);
                var newMappingContextExp = Expression.New(genericMappingContext);
                var contextVarExp = Expression.Variable(genericMappingContext, "context");
                var assignContextExp = Expression.Assign(contextVarExp, newMappingContextExp);

                var sourceExp = Expression.Property(contextVarExp, "Source");
                var sourceAssignedExp = Expression.Assign(sourceExp, srcTypedExp);

                var mapCall = Expression.Call(genVariable, methodInfo, contextVarExp);
                var resultVarExp = Expression.Variable(dstType, "result");
                var resultAssignExp = Expression.Assign(resultVarExp, mapCall);

                //var blockExpression = Expression.Block(new[] {srcTypedExp, genVariable, resultVarExp},
                var blockExpression = Expression.Block(new[] { genVariable, contextVarExp }, assignExp, assignContextExp, sourceAssignedExp, resultAssignExp);


                CustomTypeMapperExpCache[cacheKey] = Expression.Block(new ParameterExpression[] { }, blockExpression);
            }
        }

        private static void CompileGenericCustomTypeMapperWithDestination(Type srcType, Type dstType, ICustomTypeMapper typeMapper, int cacheKey)
        {
            if (CustomTypeMapperWithDestExpCache.ContainsKey(cacheKey)) return;

            var srcTypedExp = Expression.Variable(srcType, "srcTyped");
            var dstTypedExp = Expression.Variable(dstType, "dstTyped");

            var customGenericType = typeof(ICustomTypeMapper<,>).MakeGenericType(srcType, dstType);
            var castToCustomGeneric = Expression.Convert(
                Expression.Constant(typeMapper, typeof(ICustomTypeMapper)),
                customGenericType);
            var genVariable = Expression.Variable(customGenericType);
            var assignExp = Expression.Assign(genVariable, castToCustomGeneric);
            var methodInfo = customGenericType.GetMethod("Map");
            var genericMappingContext = typeof(DefaultMappingContext<,>).MakeGenericType(srcType, dstType);
            var newMappingContextExp = Expression.New(genericMappingContext);
            var contextVarExp = Expression.Variable(genericMappingContext, "context");
            var assignContextExp = Expression.Assign(contextVarExp, newMappingContextExp);

            var sourceExp = Expression.Property(contextVarExp, "Source");
            var sourceAssignedExp = Expression.Assign(sourceExp, srcTypedExp);
            var destExp = Expression.Property(contextVarExp, "Destination");
            var destAssignedExp = Expression.Assign(destExp, dstTypedExp);

            var mapCall = Expression.Call(genVariable, methodInfo, contextVarExp);
            var resultVarExp = Expression.Variable(dstType, "result");
            var resultAssignExp = Expression.Assign(resultVarExp, mapCall);

            //var blockExpression = Expression.Block(new[] { srcTypedExp, dstTypedExp, genVariable, resultVarExp },
            var blockExpression = Expression.Block(new[] { genVariable, contextVarExp }, assignExp, assignContextExp, sourceAssignedExp, destAssignedExp, resultAssignExp);

            CustomTypeMapperWithDestExpCache[cacheKey] = Expression.Block(new ParameterExpression[] { }, blockExpression);
        }

        internal static BlockExpression GetCustomMapExpression(Type src, Type dest, bool withDestination = false)
        {
            var cacheKey = CalculateCacheKey(src, dest);
            if (!CustomMappers.ContainsKey(cacheKey)) return null;
            CompileGenericCustomTypeMapper(src, dest, CustomMappers[cacheKey](), cacheKey);
            CompileGenericCustomTypeMapperWithDestination(src, dest, CustomMappers[cacheKey](), cacheKey);
            return withDestination ? CustomTypeMapperWithDestExpCache[cacheKey] : CustomTypeMapperExpCache[cacheKey];
        }

        private static int CalculateCacheKey(Type source, Type dest)
        {
            var srcHash = source.GetHashCode();
            return srcHash + dest.GetHashCode()/srcHash;
        }

        #endregion

    }
}

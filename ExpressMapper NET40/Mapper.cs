using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.ObjectModel;

namespace ExpressMapper
{
    public static class Mapper
    {
        public static bool IsDestinationInstance = false;

        private readonly static Dictionary<int, ITypeMapper> TypeMappers = new Dictionary<int, ITypeMapper>();
        private readonly static Dictionary<int, Func<ICustomTypeMapper>> CustomMappers = new Dictionary<int, Func<ICustomTypeMapper>>();
        private readonly static Dictionary<int, MulticastDelegate> CustomSimpleMappers = new Dictionary<int, MulticastDelegate>();
        private readonly static Dictionary<int, MulticastDelegate> CustomSimpleMappersWithDest = new Dictionary<int, MulticastDelegate>();

        private readonly static Dictionary<int, MulticastDelegate> CollectionMappers = new Dictionary<int, MulticastDelegate>();
        private readonly static Dictionary<int, MulticastDelegate> CollectionMappersWithDest = new Dictionary<int, MulticastDelegate>();

        private readonly static Dictionary<int, Func<object, object>> CustomTypeMapperCache = new Dictionary<int, Func<object, object>>();
        private readonly static Dictionary<int, Func<object, object, object>> CustomTypeMapperWithDestCache = new Dictionary<int, Func<object, object, object>>();

        private readonly static Dictionary<int, BlockExpression> CustomTypeMapperExpCache = new Dictionary<int, BlockExpression>();
        private readonly static Dictionary<int, BlockExpression> CustomTypeMapperWithDestExpCache = new Dictionary<int, BlockExpression>();


        public static IMemberConfiguration<T, TN> Register<T, TN>()
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
            
            CustomMappers.Clear();
            CustomSimpleMappers.Clear();
            CustomSimpleMappersWithDest.Clear();
            
            CollectionMappers.Clear();
            CollectionMappersWithDest.Clear();
            
            CustomTypeMapperCache.Clear();
            CustomTypeMapperWithDestCache.Clear();
            
            CustomTypeMapperExpCache.Clear();
            CustomTypeMapperWithDestExpCache.Clear();
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
                var typeMapper = customTypeMapper() as ICustomTypeMapper<T, TN>;
                var context = new DefaultMappingContext<T, TN> { Source = src };
                return typeMapper.Map(context);
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
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                (typeof(TN).IsGenericType && typeof(TN).GetInterfaces().Any(t => t == typeof(IEnumerable)) ? typeof(TN)
                    : null);

            if (tCol != null && tnCol != null)
            {
                if (!CollectionMappers.ContainsKey(cacheKey))
                {
                    PreCompileCollection<T, TN>();
                }
                return (TN)CollectionMappers[cacheKey].DynamicInvoke(src);
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
                var defaultMappingContext = new DefaultMappingContext<T, TN> { Source = src, Destination = dest };
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
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                (typeof(TN).IsGenericType && typeof(TN).GetInterfaces().Any(t => t == typeof(IEnumerable)) ? typeof(TN)
                    : null);

            if (tCol != null && tnCol != null)
            {
                if (!CollectionMappers.ContainsKey(cacheKey))
                {
                    PreCompileCollection<T, TN>();
                }
                return (TN)CollectionMappersWithDest[cacheKey].DynamicInvoke(src, dest);
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
            var parameterExpression = Expression.Parameter(typeof(object), "src");
            var srcConverted = Expression.Convert(parameterExpression, srcType);
            var srcTypedExp = Expression.Variable(srcType, "srcTyped");
            var srcAssigned = Expression.Assign(srcTypedExp, srcConverted);

            var customGenericType = typeof(ICustomTypeMapper<,>).MakeGenericType(srcType, dstType);
            var castToCustomGeneric = Expression.Convert(Expression.Constant(typeMapper, typeof(ICustomTypeMapper)),
                customGenericType);
            var genVariable = Expression.Variable(customGenericType);
            var assignExp = Expression.Assign(genVariable, castToCustomGeneric);
            var methodInfo = customGenericType.GetMethod("Map");
            var genericMappingContext = typeof(DefaultMappingContext<,>).MakeGenericType(srcType, dstType);
            var newMappingContextExp = Expression.New(genericMappingContext);

            var contextVarExp = Expression.Variable(genericMappingContext, string.Format("context{0}", Guid.NewGuid()));
            var assignContextExp = Expression.Assign(contextVarExp, newMappingContextExp);

            var sourceExp = Expression.Property(contextVarExp, "Source");
            var sourceAssignedExp = Expression.Assign(sourceExp, srcTypedExp);

            var mapCall = Expression.Call(genVariable, methodInfo, contextVarExp);
            var resultVarExp = Expression.Variable(typeof(object), "result");
            var resultAssignExp = Expression.Assign(resultVarExp, Expression.Convert(mapCall, typeof(object)));

            var blockExpression = Expression.Block(new[] { srcTypedExp, genVariable, contextVarExp, resultVarExp },
                new Expression[] { srcAssigned, assignExp, assignContextExp, sourceAssignedExp, resultAssignExp, resultVarExp });

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

            var contextVarExp = Expression.Variable(genericMappingContext, string.Format("context{0}", Guid.NewGuid()));
            var assignContextExp = Expression.Assign(contextVarExp, newMappingContextExp);

            var sourceExp = Expression.Property(assignContextExp, "Source");
            var sourceAssignedExp = Expression.Assign(sourceExp, srcTypedExp);
            var destinationAssignedExp = Expression.Assign(destinationExpression, dstTypedExp);

            var mapCall = Expression.Call(genVariable, methodInfo, contextVarExp);
            var resultVarExp = Expression.Variable(typeof(object), "result");
            var resultAssignExp = Expression.Assign(resultVarExp, Expression.Convert(mapCall, typeof(object)));

            var blockExpression = Expression.Block(new[] {srcTypedExp, genVariable, contextVarExp, resultVarExp},
                assignExp, sourceAssignedExp, assignContextExp, destinationAssignedExp, resultAssignExp, resultVarExp);

            var lambda = Expression.Lambda<Func<object, object, object>>(blockExpression, sourceExpression, destinationExpression);
            CustomTypeMapperWithDestCache.Add(cacheKey, lambda.Compile());
        }

        private static void CompileGenericCustomTypeMapper(Type srcType, Type dstType, ICustomTypeMapper typeMapper, int cacheKey)
        {
            if (!CustomTypeMapperExpCache.ContainsKey(cacheKey))
            {
                var srcTypedExp = Expression.Variable(srcType, "srcTyped");

                var customGenericType = typeof(ICustomTypeMapper<,>).MakeGenericType(srcType, dstType);
                var castToCustomGeneric = Expression.Convert(
                    Expression.Constant(typeMapper, typeof(ICustomTypeMapper)),
                    customGenericType);
                var genVariable = Expression.Variable(customGenericType);
                var assignExp = Expression.Assign(genVariable, castToCustomGeneric);
                var methodInfo = customGenericType.GetMethod("Map");
                var genericMappingContext = typeof(DefaultMappingContext<,>).MakeGenericType(srcType, dstType);
                var newMappingContextExp = Expression.New(genericMappingContext);
                var contextVarExp = Expression.Variable(genericMappingContext, string.Format("context{0}", Guid.NewGuid()));
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
            var contextVarExp = Expression.Variable(genericMappingContext, string.Format("context{0}", Guid.NewGuid()));
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

        public static void PreCompileCollection<T, TN>()
        {
            CompileCollection<T, TN>();
            CompileCollectionWithDestination<T, TN>();
        }

        private static void CompileCollection<T, TN>()
        {
            var sourceParameterExp = Expression.Parameter(typeof(T), "sourceColl");
            var blockExp = CompileCollectionInternal<T, TN>(sourceParameterExp);
            var lambda = Expression.Lambda<Func<T, TN>>(blockExp, sourceParameterExp);
            var compiledFunc = lambda.Compile();
            var cacheKey = CalculateCacheKey(typeof(T), typeof(TN));
            CollectionMappers.Add(cacheKey, compiledFunc);
        }

        private static BlockExpression CompileCollectionInternal<T, TN>(ParameterExpression sourceParameterExp, ParameterExpression destParameterExp = null)
        {
            var sourceType = typeof(T).GetGenericArguments()[0];
            var destType = typeof(TN).GetGenericArguments()[0];

            var destList = typeof(List<>).MakeGenericType(destType);
            var destColl = Expression.Variable(destList, "destColl");

            var constructorInfo = destList.GetConstructors().First(c => c.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(int)) != null);
            var srcCountExp = Expression.Call(typeof(Enumerable), "Count", new[] { sourceType }, sourceParameterExp);

            var newColl = Expression.New(constructorInfo, srcCountExp);
            var destAssign = Expression.Assign(destColl, newColl);

            var closedEnumeratorSourceType = typeof(IEnumerator<>).MakeGenericType(sourceType);
            var closedEnumerableSourceType = typeof(IEnumerable<>).MakeGenericType(sourceType);
            var enumerator = Expression.Variable(closedEnumeratorSourceType, "srcEnumerator");
            var assignToEnum = Expression.Assign(enumerator,
                Expression.Call(sourceParameterExp, closedEnumerableSourceType.GetMethod("GetEnumerator")));
            var doMoveNext = Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext"));

            var current = Expression.Property(enumerator, "Current");
            var sourceColItmVariable = Expression.Variable(sourceType, "ItmSrc");
            var assignSourceItmFromProp = Expression.Assign(sourceColItmVariable, current);

            var destColItmVariable = Expression.Variable(destType, "ItmDest");
            var mapExprForType = Mapper.GetMapExpressions(sourceType, destType);
            var blockForSubstiyution = Expression.Block(mapExprForType);
            var substBlock =
                new SubstituteParameterVisitor(sourceColItmVariable, destColItmVariable).Visit(
                    blockForSubstiyution) as BlockExpression;
            var resultMapExprForType = substBlock.Expressions;

            var addToNewColl = Expression.Call(destColl, "Add", null, destColItmVariable);
            var blockExps = new List<Expression> { assignSourceItmFromProp };
            blockExps.AddRange(resultMapExprForType);
            blockExps.Add(addToNewColl);

            var ifTrueBlock = Expression.Block(new[] { sourceColItmVariable, destColItmVariable }, blockExps);

            var brk = Expression.Label();
            var loopExpression = Expression.Loop(
                Expression.IfThenElse(
                    Expression.NotEqual(doMoveNext, Expression.Constant(false)),
                    ifTrueBlock
                    , Expression.Break(brk))
                , brk);

            Expression resultCollection = destColl;
            if (typeof(TN).IsArray)
            {
                resultCollection = Expression.Call(destColl, destList.GetMethod("ToArray"));
            }
            else
            {
                if (typeof(TN).IsGenericType && typeof(TN).GetInterfaces().Any(t => t == typeof(IQueryable)))
                {
                    resultCollection = Expression.Call(typeof(Queryable), "AsQueryable", new[] { destType }, destColl);
                }
            }

            var parameters = new List<ParameterExpression>{destColl, enumerator};

            var expressions = new List<Expression>
            {
                destAssign,
                assignToEnum,
                loopExpression,
                destParameterExp != null
                    ? Expression.Assign(destParameterExp, resultCollection)
                    : resultCollection
            };


            var blockExpression = Expression.Block(parameters, expressions);
            return blockExpression;
        }

        private static void CompileCollectionWithDestination<T, TN>()
        {
            var sourceType = typeof(T).GetGenericArguments()[0];
            var destType = typeof(TN).GetGenericArguments()[0];
            var sourceVariable = Expression.Parameter(typeof(T), "source");
            var destVariable = Expression.Parameter(typeof(TN), "dest");

            var srcCount = Expression.Call(typeof(Enumerable), "Count", new[] { sourceType }, sourceVariable);
            var destCount = Expression.Call(typeof(Enumerable), "Count", new[] { destType }, destVariable);

            var conditionToCreateList = Expression.NotEqual(srcCount, destCount);
            var notNullCondition = Expression.IfThenElse(conditionToCreateList,
                MapCollectionNotCountEquals(typeof(T), typeof(TN), sourceVariable, destVariable),
                MapCollectionCountEquals(typeof(T), typeof(TN), sourceVariable, destVariable));

            var newCollBlockExp = CompileCollectionInternal<T, TN>(sourceVariable, destVariable);

            var result = Expression.IfThenElse(Expression.NotEqual(destVariable, Expression.Constant(null)), notNullCondition,
                newCollBlockExp);

            //			var blockExpression = Expression.Block(new ParameterExpression[]{}, new Expression[]{result});
            //			var expression = new SubstituteParameterVisitor(sourceVariable).Visit(blockExpression) as BlockExpression;

            var expressions = new List<Expression> { result };

            var resultExpression = Expression.Block(new ParameterExpression[] { }, expressions);

            var checkSrcForNullExp =
                Expression.IfThenElse(Expression.Equal(sourceVariable, Expression.Constant(null)),
                    Expression.Default(destVariable.Type), resultExpression);
            var block = Expression.Block(new ParameterExpression[] { }, checkSrcForNullExp, destVariable);
            var lambda = Expression.Lambda<Func<T, TN, TN>>(block, sourceVariable, destVariable);
            var compiledFunc = lambda.Compile();

            var cacheKey = CalculateCacheKey(typeof(T), typeof(TN));
            CollectionMappersWithDest.Add(cacheKey, compiledFunc);

        }

        private static BlockExpression MapCollectionCountEquals(Type tCol, Type tnCol, Expression sourceVariable, Expression destVariable)
        {
            var sourceType = tCol.GetGenericArguments()[0];
            var destType = tnCol.GetGenericArguments()[0];

            // Source enumeration
            var closedEnumeratorSourceType = typeof(IEnumerator<>).MakeGenericType(sourceType);
            var closedEnumerableSourceType = typeof(IEnumerable<>).MakeGenericType(sourceType);
            var enumeratorSrc = Expression.Variable(closedEnumeratorSourceType, "EnumSrc");
            var assignToEnumSrc = Expression.Assign(enumeratorSrc,
                Expression.Call(sourceVariable, closedEnumerableSourceType.GetMethod("GetEnumerator")));
            var doMoveNextSrc = Expression.Call(enumeratorSrc, typeof(IEnumerator).GetMethod("MoveNext"));
            var currentSrc = Expression.Property(enumeratorSrc, "Current");
            var srcItmVarExp = Expression.Variable(sourceType, "ItmSrc");
            var assignSourceItmFromProp = Expression.Assign(srcItmVarExp, currentSrc);

            // dest enumeration
            var closedEnumeratorDestType = typeof(IEnumerator<>).MakeGenericType(destType);
            var closedEnumerableDestType = typeof(IEnumerable<>).MakeGenericType(destType);
            var enumeratorDest = Expression.Variable(closedEnumeratorDestType, "EnumDest");
            var assignToEnumDest = Expression.Assign(enumeratorDest,
                Expression.Call(destVariable, closedEnumerableDestType.GetMethod("GetEnumerator")));
            var doMoveNextDest = Expression.Call(enumeratorDest, typeof(IEnumerator).GetMethod("MoveNext"));
            var currentDest = Expression.Property(enumeratorDest, "Current");
            var destItmVarExp = Expression.Variable(destType, "ItmDest");
            var assignDestItmFromProp = Expression.Assign(destItmVarExp, currentDest);

            var blockForSubstitution = Mapper.GetCustomMapExpression(sourceType, destType, true);
            if (blockForSubstitution == null)
            {
                var mapExprForType = new List<Expression>(Mapper.GetMapExpressions(sourceType, destType, true));

                var newDestInstanceExp = mapExprForType[0] as BinaryExpression;
                if (newDestInstanceExp != null)
                {
                    mapExprForType.RemoveAt(0);

                    var destCondition = Expression.IfThen(Expression.Equal(destItmVarExp, Expression.Constant(null)),
                        newDestInstanceExp);
                    mapExprForType.Insert(0, destCondition);
                }

                blockForSubstitution = Expression.Block(mapExprForType);
            }

            var substBlock =
                new SubstituteParameterVisitor(srcItmVarExp, destItmVarExp).Visit(
                    blockForSubstitution) as BlockExpression;

            var blockExps = new List<Expression> { assignSourceItmFromProp, assignDestItmFromProp };
            blockExps.AddRange(substBlock.Expressions);

            var ifTrueBlock = Expression.Block(new[] { srcItmVarExp, destItmVarExp }, blockExps);

            var brk = Expression.Label();
            var loopExpression = Expression.Loop(
                Expression.IfThenElse(
                    Expression.AndAlso(Expression.NotEqual(doMoveNextSrc, Expression.Constant(false)), Expression.NotEqual(doMoveNextDest, Expression.Constant(false))),
                    ifTrueBlock
                    , Expression.Break(brk))
                , brk);

            var parameters = new List<ParameterExpression> { enumeratorSrc, enumeratorDest };
            var expressions = new List<Expression>
			{
				assignToEnumSrc,
				assignToEnumDest,
				loopExpression
			};

            var blockExpression = Expression.Block(parameters, expressions);
            return blockExpression;
        }

        private static BlockExpression MapCollectionNotCountEquals(Type tCol, Type tnCol, Expression sourceVariable, Expression destVariable)
        {
            var sourceType = tCol.GetGenericArguments()[0];
            var destType = tnCol.GetGenericArguments()[0];

            var destList = typeof(IList<>).MakeGenericType(destType);
            var destCollection = typeof(ICollection<>).MakeGenericType(destType);

            BlockExpression resultExpression;
            var isICollection = !destVariable.Type.IsArray && (destVariable.Type.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ICollection<>)) != null ||
                destVariable.Type == destCollection);

            var srcCount = Expression.Call(typeof(Enumerable), "Count", new[] { sourceType }, sourceVariable);
            var destCount = Expression.Call(typeof(Enumerable), "Count", new[] { destType }, destVariable);

            if (isICollection)
            {
                // If it is a list and destCount greater than srcCount

                var equalsBlockExp = MapCollectionCountEquals(tCol, tnCol, sourceVariable, destVariable);

                var getFirstEnumExp = Expression.Call(typeof(Enumerable), "First", new[] { destType }, destVariable);
                var removeCollFirstExp = Expression.Call(destVariable, destCollection.GetMethod("Remove"), getFirstEnumExp);

                var brkColRem = Expression.Label();
                var loopToDropColElements = Expression.Loop(
                    Expression.IfThenElse(Expression.GreaterThan(destCount, srcCount),
                        removeCollFirstExp
                        , Expression.Break(brkColRem))
                    , brkColRem);

                var collRemoveExps = new List<Expression> { loopToDropColElements, equalsBlockExp };
                var collRemoveBlockExp = Expression.Block(new ParameterExpression[] { }, collRemoveExps);

                // List and Collection - if src count greater than dest

                var mapCollectionSourcePrevail = MapCollectionSourcePrevail(destVariable, sourceType, sourceVariable, destType);
                var collBlock = Expression.IfThenElse(Expression.GreaterThan(destCount, srcCount), collRemoveBlockExp, mapCollectionSourcePrevail);
                resultExpression = Expression.Block(new ParameterExpression[] { }, new Expression[] { collBlock });
            }
            else
            {
                // Else

                var destListType = typeof(List<>).MakeGenericType(destType);
                var destVarExp = Expression.Variable(destListType, "InterimDest");
                var constructorInfo = destListType.GetConstructors().First(c => c.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(int)) != null);

                var newColl = Expression.New(constructorInfo, srcCount);
                var destAssign = Expression.Assign(destVarExp, newColl);

                // Source enumeration
                var closedEnumeratorSourceType = typeof(IEnumerator<>).MakeGenericType(sourceType);
                var closedEnumerableSourceType = typeof(IEnumerable<>).MakeGenericType(sourceType);
                var enumeratorSrc = Expression.Variable(closedEnumeratorSourceType, "EnumSrc");
                var assignToEnumSrc = Expression.Assign(enumeratorSrc,
                    Expression.Call(sourceVariable, closedEnumerableSourceType.GetMethod("GetEnumerator")));
                var doMoveNextSrc = Expression.Call(enumeratorSrc, typeof(IEnumerator).GetMethod("MoveNext"));
                var currentSrc = Expression.Property(enumeratorSrc, "Current");

                var srcItmVarExp = Expression.Variable(sourceType, "ItmSrc");
                var assignSourceItmFromProp = Expression.Assign(srcItmVarExp, currentSrc);

                // dest enumeration
                var closedEnumeratorDestType = typeof(IEnumerator<>).MakeGenericType(destType);
                var closedEnumerableDestType = typeof(IEnumerable<>).MakeGenericType(destType);
                var enumeratorDest = Expression.Variable(closedEnumeratorDestType, "EnumDest");
                var assignToEnumDest = Expression.Assign(enumeratorDest,
                    Expression.Call(destVariable, closedEnumerableDestType.GetMethod("GetEnumerator")));
                var doMoveNextDest = Expression.Call(enumeratorDest, typeof(IEnumerator).GetMethod("MoveNext"));

                var currentDest = Expression.Property(enumeratorDest, "Current");
                var destItmVarExp = Expression.Variable(destType, "ItmDest");
                var assignDestItmFromProp = Expression.Assign(destItmVarExp, currentDest);

                var blockForSubstitution = Mapper.GetCustomMapExpression(sourceType, destType, true);
                if (blockForSubstitution == null)
                {
                    var mapExprForType = Mapper.GetMapExpressions(sourceType, destType, true);

                    var newDestInstanceExp = mapExprForType[0] as BinaryExpression;
                    if (newDestInstanceExp != null)
                    {
                        mapExprForType.RemoveAt(0);

                        var destCondition = Expression.IfThen(Expression.Equal(destItmVarExp, Expression.Constant(null)),
                            newDestInstanceExp);
                        mapExprForType.Insert(0, destCondition);
                    }

                    blockForSubstitution = Expression.Block(mapExprForType);
                }

                var substBlock =
                    new SubstituteParameterVisitor(srcItmVarExp, destItmVarExp).Visit(
                        blockForSubstitution) as BlockExpression;
                var resultMapExprForType = substBlock.Expressions;

                var blockExps = new List<Expression> { assignDestItmFromProp };
                blockExps.AddRange(resultMapExprForType);

                var ifTrueBlock = Expression.Block(new ParameterExpression[] { }, blockExps);

                // If destination list is empty

                var blockForSubstitutionNew = Mapper.GetCustomMapExpression(sourceType, destType);
                if (blockForSubstitutionNew == null)
                {
                    var mapExprForTypeNew = Mapper.GetMapExpressions(sourceType, destType);
                    blockForSubstitutionNew = Expression.Block(mapExprForTypeNew);
                }

                var substBlockNew =
                    new SubstituteParameterVisitor(srcItmVarExp, destItmVarExp).Visit(
                        blockForSubstitutionNew) as BlockExpression;
                var resultMapExprForTypeNew = substBlockNew.Expressions;

                var blockExpsNew = new List<Expression>(resultMapExprForTypeNew);

                var ifFalseBlock = Expression.Block(new ParameterExpression[] { }, blockExpsNew);

                var mapAndAddItemExp = Expression.IfThenElse(doMoveNextDest, ifTrueBlock, ifFalseBlock);
                var addToNewCollNew = Expression.Call(destVarExp, "Add", null, destItmVarExp);

                var innerLoopBlock = Expression.Block(new[] { srcItmVarExp, destItmVarExp },
                    new Expression[] { assignSourceItmFromProp, mapAndAddItemExp, addToNewCollNew });

                var brk = Expression.Label();
                var loopExpression = Expression.Loop(
                    Expression.IfThenElse(Expression.NotEqual(doMoveNextSrc, Expression.Constant(false)),
                        innerLoopBlock
                        , Expression.Break(brk))
                    , brk);

                Expression resultCollection = destVarExp;
                if (destVariable.Type.IsArray)
                {
                    resultCollection = Expression.Call(destVarExp, destListType.GetMethod("ToArray"));
                }
                else
                {
                    if (destVariable.Type.IsGenericType && destVariable.Type.GetInterfaces().Any(t => t == typeof(IQueryable)))
                    {
                        resultCollection = Expression.Call(typeof(Queryable), "AsQueryable", new[] { destType }, destVarExp);
                    }
                    else
                    {
                        if (destVariable.Type.IsGenericType && destVariable.Type == typeof(Collection<>).MakeGenericType(destType))
                        {
                            resultCollection = Expression.Call(typeof(CollectionExtentions), "ToCollection", new[] { destType }, destVarExp);
                        }
                    }
                }

                var assignResult = Expression.Assign(destVariable, resultCollection);

                var parameters = new List<ParameterExpression> { destVarExp, enumeratorSrc, enumeratorDest };
                var expressions = new List<Expression>
				{
					destAssign,
					assignToEnumSrc,
					assignToEnumDest,
					loopExpression,
					assignResult
				};

                resultExpression = Expression.Block(parameters, expressions);
            }

            return resultExpression;
        }

        private static BlockExpression MapCollectionSourcePrevail(Expression destVariable, Type sourceType, Expression sourceVariable, Type destType)
        {
            // Source enumeration
            var closedEnumeratorSourceType = typeof(IEnumerator<>).MakeGenericType(sourceType);
            var closedEnumerableSourceType = typeof(IEnumerable<>).MakeGenericType(sourceType);
            var enumeratorSrc = Expression.Variable(closedEnumeratorSourceType, "EnumSrc");
            var assignToEnumSrc = Expression.Assign(enumeratorSrc,
                Expression.Call(sourceVariable, closedEnumerableSourceType.GetMethod("GetEnumerator")));
            var doMoveNextSrc = Expression.Call(enumeratorSrc, typeof(IEnumerator).GetMethod("MoveNext"));
            var currentSrc = Expression.Property(enumeratorSrc, "Current");

            var srcItmVarExp = Expression.Variable(sourceType, "ItmSrc");
            var assignSourceItmFromProp = Expression.Assign(srcItmVarExp, currentSrc);

            // dest enumeration
            var closedEnumeratorDestType = typeof(IEnumerator<>).MakeGenericType(destType);
            var closedEnumerableDestType = typeof(IEnumerable<>).MakeGenericType(destType);
            var enumeratorDest = Expression.Variable(closedEnumeratorDestType, "EnumDest");
            var assignToEnumDest = Expression.Assign(enumeratorDest,
                Expression.Call(destVariable, closedEnumerableDestType.GetMethod("GetEnumerator")));
            var doMoveNextDest = Expression.Call(enumeratorDest, typeof(IEnumerator).GetMethod("MoveNext"));

            var currentDest = Expression.Property(enumeratorDest, "Current");
            var destItmVarExp = Expression.Variable(destType, "ItmDest");
            var assignDestItmFromProp = Expression.Assign(destItmVarExp, currentDest);

            var blockForSubstitution = Mapper.GetCustomMapExpression(sourceType, destType, true);
            if (blockForSubstitution == null)
            {
                var mapExprForType = Mapper.GetMapExpressions(sourceType, destType, true);

                var newDestInstanceExp = mapExprForType[0] as BinaryExpression;
                if (newDestInstanceExp != null)
                {
                    mapExprForType.RemoveAt(0);

                    var destCondition = Expression.IfThen(Expression.Equal(destItmVarExp, Expression.Constant(null)),
                        newDestInstanceExp);
                    mapExprForType.Insert(0, destCondition);
                }

                blockForSubstitution = Expression.Block(mapExprForType);
            }

            var substBlock =
                new SubstituteParameterVisitor(srcItmVarExp, destItmVarExp).Visit(
                    blockForSubstitution) as BlockExpression;
            var resultMapExprForType = substBlock.Expressions;

            var blockExps = new List<Expression> { assignDestItmFromProp };
            blockExps.AddRange(resultMapExprForType);

            var ifTrueBlock = Expression.Block(new ParameterExpression[] { }, blockExps);

            // If destination list is empty
            var blockForSubstitutionNew = Mapper.GetCustomMapExpression(sourceType, destType);
            if (blockForSubstitutionNew == null)
            {
                var mapExprForTypeNew = Mapper.GetMapExpressions(sourceType, destType);
                blockForSubstitutionNew = Expression.Block(mapExprForTypeNew);
            }

            var substBlockNew =
                new SubstituteParameterVisitor(srcItmVarExp, destItmVarExp).Visit(
                    blockForSubstitutionNew) as BlockExpression;
            var resultMapExprForTypeNew = substBlockNew.Expressions;

            var destCollection = typeof(ICollection<>).MakeGenericType(destType);

            var addToNewCollNew = Expression.Call(destVariable, destCollection.GetMethod("Add"), destItmVarExp);
            var blockExpsNew = new List<Expression>(resultMapExprForTypeNew) { addToNewCollNew };

            var ifFalseBlock = Expression.Block(new ParameterExpression[] { }, blockExpsNew);

            var endOfListExp = Expression.Variable(typeof(bool), "endOfList");
            var assignInitEndOfListExp = Expression.Assign(endOfListExp, Expression.Constant(false));

            var ifNotEndOfListExp = Expression.IfThen(Expression.Equal(endOfListExp, Expression.Constant(false)), Expression.Assign(endOfListExp, Expression.Not(doMoveNextDest)));

            var mapAndAddItemExp = Expression.IfThenElse(endOfListExp, ifFalseBlock, ifTrueBlock);

            var innerLoopBlock = Expression.Block(new[] { srcItmVarExp, destItmVarExp },
                new Expression[] { assignSourceItmFromProp, ifNotEndOfListExp, mapAndAddItemExp });

            var brk = Expression.Label();
            var loopExpression = Expression.Loop(
                Expression.IfThenElse(Expression.NotEqual(doMoveNextSrc, Expression.Constant(false)),
                    innerLoopBlock
                    , Expression.Break(brk))
                , brk);

            var blockExpression = Expression.Block(new[] { endOfListExp, enumeratorSrc, enumeratorDest }, new Expression[] { assignInitEndOfListExp, assignToEnumSrc, assignToEnumDest, loopExpression });
            return blockExpression;
        }

        private static int CalculateCacheKey(Type source, Type dest)
        {
            var srcHash = source.GetHashCode();
            return srcHash + dest.GetHashCode() / srcHash;
        }

        #endregion

    }
}

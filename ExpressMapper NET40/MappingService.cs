using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressMapper
{
    public sealed class MappingService : IMappingService
    {
        private readonly Dictionary<int, ITypeMapper> TypeMappers = new Dictionary<int, ITypeMapper>();
        private readonly Dictionary<int, Func<ICustomTypeMapper>> CustomMappers = new Dictionary<int, Func<ICustomTypeMapper>>();
        private readonly Dictionary<int, MulticastDelegate> CustomSimpleMappers = new Dictionary<int, MulticastDelegate>();
        private readonly Dictionary<int, MulticastDelegate> CustomSimpleMappersWithDest = new Dictionary<int, MulticastDelegate>();

        private readonly Dictionary<int, MulticastDelegate> CollectionMappers = new Dictionary<int, MulticastDelegate>();
        private readonly Dictionary<int, MulticastDelegate> CollectionMappersWithDest = new Dictionary<int, MulticastDelegate>();

        private readonly Dictionary<int, Func<object, object>> CustomTypeMapperCache = new Dictionary<int, Func<object, object>>();
        private readonly Dictionary<int, Func<object, object, object>> CustomTypeMapperWithDestCache = new Dictionary<int, Func<object, object, object>>();

        private readonly Dictionary<int, BlockExpression> CustomTypeMapperExpCache = new Dictionary<int, BlockExpression>();
        private readonly Dictionary<int, BlockExpression> CustomTypeMapperWithDestExpCache = new Dictionary<int, BlockExpression>();

        private readonly List<int> NonGenericCollectionMappingCache = new List<int>();

        public IMemberConfiguration<T, TN> Register<T, TN>()
        {
            Type src = typeof(T);
            Type dest = typeof(TN);
            var cacheKey = CalculateCacheKey(src, dest);

            if (TypeMappers.ContainsKey(cacheKey))
            {
                throw new InvalidOperationException(String.Format("Mapping from {0} to {1} is already registered", src.FullName, dest.FullName));
            }

            var classMapper = new TypeMapper<T, TN>(this);
            TypeMappers[cacheKey] = classMapper;
            return new MemberConfiguration<T, TN>(classMapper);
        }

        private List<Expression> GetMapExpressions(Type src, Type dest, bool withDestinationInstance = false)
        {
            var cacheKey = CalculateCacheKey(src, dest);
            if (TypeMappers.ContainsKey(cacheKey))
            {
                return TypeMappers[cacheKey].GetMapExpressions(withDestinationInstance);
            }
            throw new MapNotImplementedException(string.Format("There is no mapping has bee found. Source Type: {0}, Destination Type: {1}", src.FullName, dest.FullName));
        }

        public void Compile()
        {
            foreach (var typeMapper in TypeMappers)
            {
                typeMapper.Value.Compile();
                typeMapper.Value.CompileDestinationInstance();
            }
        }

        public void Reset()
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

        public void RegisterCustom<T, TN>(Func<T, TN> mapFunc)
        {
            Type src = typeof(T);
            Type dest = typeof(TN);
            var cacheKey = CalculateCacheKey(src, dest);

            if (CustomSimpleMappers.ContainsKey(cacheKey))
            {
                throw new InvalidOperationException(String.Format("Mapping from {0} to {1} is already registered", src.FullName, dest.FullName));
            }

            CustomSimpleMappers.Add(cacheKey, mapFunc);
        }

        public void RegisterCustom<T, TN, TMapper>() where TMapper : ICustomTypeMapper<T, TN>
        {
            Type src = typeof(T);
            Type dest = typeof(TN);
            var cacheKey = CalculateCacheKey(src, dest);

            if (CustomMappers.ContainsKey(cacheKey))
            {
                throw new InvalidOperationException(String.Format("Mapping from {0} to {1} is already registered", src.FullName, dest.FullName));
            }

            var newExpression = Expression.New(typeof(TMapper));
            var newLambda = Expression.Lambda<Func<ICustomTypeMapper<T, TN>>>(newExpression);
            var compile = newLambda.Compile();
            CustomMappers.Add(cacheKey, compile);
        }

        public TN Map<T, TN>(T src)
        {
            Type srcType = typeof(T);
            Type destType = typeof(TN);
            var cacheKey = CalculateCacheKey(srcType, destType);

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

            throw new MapNotImplementedException(string.Format("There is no mapping has bee found. Source Type: {0}, Destination Type: {1}", srcType.FullName, destType.FullName));
        }

        public TN Map<T, TN>(T src, TN dest)
        {
            Type srcType = typeof(T);
            Type destType = typeof(TN);
            var cacheKey = CalculateCacheKey(srcType, destType);

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

            throw new MapNotImplementedException(string.Format("There is no mapping has bee found. Source Type: {0}, Destination Type: {1}", srcType.FullName, destType.FullName));
        }

        public object Map(object src, Type srcType, Type dstType)
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

            var tCol =
                srcType.GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                    (srcType.IsGenericType
                        && srcType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? srcType
                        : null);

            var tnCol = dstType.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                         (dstType.IsGenericType && dstType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? dstType
                             : null);

            if (tCol != null && tnCol != null)
            {
                if (!CollectionMappers.ContainsKey(cacheKey))
                {
                    CompileNonGenericCollectionMapping(srcType, dstType);
                }
                return CollectionMappers[cacheKey].DynamicInvoke(src);
            }

            throw new MapNotImplementedException(string.Format("There is no mapping has bee found. Source Type: {0}, Destination Type: {1}", srcType.FullName, dstType.FullName));
        }

        public object Map(object src, object dest, Type srcType, Type dstType)
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

            var tCol =
                srcType.GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                    (srcType.IsGenericType
                        && srcType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? srcType
                        : null);

            var tnCol = dstType.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                         (dstType.IsGenericType && dstType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? dstType
                             : null);

            if (tCol != null && tnCol != null)
            {
                if (!CollectionMappers.ContainsKey(cacheKey))
                {
                    CompileNonGenericCollectionMapping(srcType, dstType);
                }
                return CollectionMappersWithDest[cacheKey].DynamicInvoke(src, dest);
            }
            throw new MapNotImplementedException(string.Format("There is no mapping has bee found. Source Type: {0}, Destination Type: {1}", srcType.FullName, dstType.FullName));
        }

        #region Helper methods

        private void CompileNonGenericCustomTypeMapper(Type srcType, Type dstType, ICustomTypeMapper typeMapper, int cacheKey)
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

        private void CompileNonGenericCollectionMapping(Type srcType, Type dstType)
        {
            var cacheKey = CalculateCacheKey(srcType, dstType);
            if (NonGenericCollectionMappingCache.Contains(cacheKey)) return;

            var methodInfo = GetType().GetMethod("PreCompileCollection");
            var makeGenericMethod = methodInfo.MakeGenericMethod(srcType, dstType);
            var methodCallExpression = Expression.Call(Expression.Constant(this), makeGenericMethod);
            var expression = Expression.Lambda<Action>(methodCallExpression);
            var action = expression.Compile();
            action();
        }

        private void CompileNonGenericCustomTypeMapperWithDestination(Type srcType, Type dstType, ICustomTypeMapper typeMapper, int cacheKey)
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

            var blockExpression = Expression.Block(new[] { srcTypedExp, genVariable, contextVarExp, resultVarExp },
                assignExp, sourceAssignedExp, assignContextExp, destinationAssignedExp, resultAssignExp, resultVarExp);

            var lambda = Expression.Lambda<Func<object, object, object>>(blockExpression, sourceExpression, destinationExpression);
            CustomTypeMapperWithDestCache.Add(cacheKey, lambda.Compile());
        }

        private void CompileGenericCustomTypeMapper(Type srcType, Type dstType, ICustomTypeMapper typeMapper, int cacheKey)
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

        private void CompileGenericCustomTypeMapperWithDestination(Type srcType, Type dstType, ICustomTypeMapper typeMapper, int cacheKey)
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

        private BlockExpression GetCustomMapExpression(Type src, Type dest, bool withDestination = false)
        {
            var cacheKey = CalculateCacheKey(src, dest);
            if (!CustomMappers.ContainsKey(cacheKey)) return null;
            CompileGenericCustomTypeMapper(src, dest, CustomMappers[cacheKey](), cacheKey);
            CompileGenericCustomTypeMapperWithDestination(src, dest, CustomMappers[cacheKey](), cacheKey);
            return withDestination ? CustomTypeMapperWithDestExpCache[cacheKey] : CustomTypeMapperExpCache[cacheKey];
        }

        public void PreCompileCollection<T, TN>()
        {
            CompileCollection<T, TN>();
            CompileCollectionWithDestination<T, TN>();
        }

        private void CompileCollection<T, TN>()
        {
            var sourceParameterExp = Expression.Parameter(typeof(T), "sourceColl");
            var blockExp = CompileCollectionInternal<T, TN>(sourceParameterExp);
            var lambda = Expression.Lambda<Func<T, TN>>(blockExp, sourceParameterExp);
            var compiledFunc = lambda.Compile();
            var cacheKey = CalculateCacheKey(typeof(T), typeof(TN));
            CollectionMappers.Add(cacheKey, compiledFunc);
        }

        private BlockExpression CompileCollectionInternal<T, TN>(ParameterExpression sourceParameterExp, ParameterExpression destParameterExp = null)
        {
            var sourceType = GetCollectionElementType(typeof(T));
            var destType = GetCollectionElementType(typeof(TN));

            var destList = typeof(List<>).MakeGenericType(destType);
            var destColl = Expression.Variable(destList, "destColl");

            var newColl = Expression.New(destList);
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

            var loopExpression = CollectionLoopExpression(sourceType, destType, destColl, sourceColItmVariable, destColItmVariable,
                assignSourceItmFromProp, doMoveNext);

            Expression resultCollection = ConvertCollection(typeof(TN), destList, destType, destColl);

            var parameters = new List<ParameterExpression> { destColl, enumerator };

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

        internal static Type GetCollectionElementType(Type type)
        {
            return type.IsArray ? type.GetElementType() : type.GetGenericArguments()[0];
        }

        private void CompileCollectionWithDestination<T, TN>()
        {
            var sourceType = GetCollectionElementType(typeof(T));
            var destType = GetCollectionElementType(typeof(TN));

            var sourceVariable = Expression.Parameter(typeof(T), "source");
            var destVariable = Expression.Parameter(typeof(TN), "dest");

            var srcCount = Expression.Call(typeof(Enumerable), "Count", new[] { sourceType }, sourceVariable);
            var destCount = Expression.Call(typeof(Enumerable), "Count", new[] { destType }, destVariable);

            var conditionToCreateList = Expression.NotEqual(srcCount, destCount);
            var notNullCondition = Expression.IfThenElse(conditionToCreateList,
                MapCollectionNotCountEquals(typeof(T), typeof(TN), sourceVariable, destVariable),
                MapCollectionCountEquals(typeof(T), typeof(TN), sourceVariable, destVariable));

            var newCollBlockExp = CompileCollectionInternal<T, TN>(sourceVariable, destVariable);

            var result = Expression.IfThenElse(Expression.NotEqual(destVariable, StaticExpressions.NullConstant), notNullCondition,
                newCollBlockExp);

            //			var blockExpression = Expression.Block(new ParameterExpression[]{}, new Expression[]{result});
            //			var expression = new SubstituteParameterVisitor(sourceVariable).Visit(blockExpression) as BlockExpression;

            var expressions = new List<Expression> { result };

            var resultExpression = Expression.Block(new ParameterExpression[] { }, expressions);

            var checkSrcForNullExp =
                Expression.IfThenElse(Expression.Equal(sourceVariable, StaticExpressions.NullConstant),
                    Expression.Default(destVariable.Type), resultExpression);
            var block = Expression.Block(new ParameterExpression[] { }, checkSrcForNullExp, destVariable);
            var lambda = Expression.Lambda<Func<T, TN, TN>>(block, sourceVariable, destVariable);
            var compiledFunc = lambda.Compile();

            var cacheKey = CalculateCacheKey(typeof(T), typeof(TN));
            CollectionMappersWithDest.Add(cacheKey, compiledFunc);

        }

        private BlockExpression MapCollectionCountEquals(Type tCol, Type tnCol, Expression sourceVariable, Expression destVariable)
        {
            var sourceType = GetCollectionElementType(tCol);
            var destType = GetCollectionElementType(tnCol);

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

            var mapExprForType = GetMemberMappingExpression(destItmVarExp, srcItmVarExp).Item2;

            var ifTrueBlock = Expression.Block(new[] { srcItmVarExp, destItmVarExp }, new[] { assignSourceItmFromProp, assignDestItmFromProp, mapExprForType });

            var brk = Expression.Label();
            var loopExpression = Expression.Loop(
                Expression.IfThenElse(
                    Expression.AndAlso(Expression.NotEqual(doMoveNextSrc, StaticExpressions.FalseConstant), Expression.NotEqual(doMoveNextDest, StaticExpressions.FalseConstant)),
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

        internal BlockExpression MapCollectionNotCountEquals(Type tCol, Type tnCol, Expression sourceVariable, Expression destVariable)
        {
            var sourceType = GetCollectionElementType(tCol);
            var destType = GetCollectionElementType(tnCol);

            var destList = typeof(List<>).MakeGenericType(destType);
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
                var constructorInfo = destListType.GetConstructor(new Type[] { typeof(int) });

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


                // If destination list is empty
                var mapExprForType = GetMemberMappingExpression(destItmVarExp, srcItmVarExp).Item1;

                var ifFalseBlock = Expression.Block(new ParameterExpression[] { }, new[] { mapExprForType });

                var ifTrueBlock = IfElseExpr(srcItmVarExp, destItmVarExp, assignDestItmFromProp);

                var mapAndAddItemExp = Expression.IfThenElse(doMoveNextDest, ifTrueBlock, ifFalseBlock);
                var addToNewCollNew = Expression.Call(destVarExp, "Add", null, destItmVarExp);

                var innerLoopBlock = Expression.Block(new[] { srcItmVarExp, destItmVarExp },
                    new Expression[] { assignSourceItmFromProp, mapAndAddItemExp, addToNewCollNew });

                var brk = Expression.Label();
                var loopExpression = Expression.Loop(
                    Expression.IfThenElse(Expression.NotEqual(doMoveNextSrc, StaticExpressions.FalseConstant),
                        innerLoopBlock
                        , Expression.Break(brk))
                    , brk);

                Expression resultCollection = ConvertCollection(destVariable.Type, destList, destType, destVarExp);

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

        internal BlockExpression MapCollectionSourcePrevail(Expression destVariable, Type sourceType, Expression sourceVariable, Type destType)
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

            var ifTrueBlock = IfElseExpr(srcItmVarExp, destItmVarExp, assignDestItmFromProp);

            // If destination list is empty
            var mapExprForType = GetMemberMappingExpression(destItmVarExp, srcItmVarExp).Item1;

            var destCollection = typeof(ICollection<>).MakeGenericType(destType);

            var addToNewCollNew = Expression.Call(destVariable, destCollection.GetMethod("Add"), destItmVarExp);

            var ifFalseBlock = Expression.Block(new ParameterExpression[] { }, new[] { mapExprForType, addToNewCollNew });

            var endOfListExp = Expression.Variable(typeof(bool), "endOfList");
            var assignInitEndOfListExp = Expression.Assign(endOfListExp, StaticExpressions.FalseConstant);

            var ifNotEndOfListExp = Expression.IfThen(Expression.Equal(endOfListExp, StaticExpressions.FalseConstant), Expression.Assign(endOfListExp, Expression.Not(doMoveNextDest)));

            var mapAndAddItemExp = Expression.IfThenElse(endOfListExp, ifFalseBlock, ifTrueBlock);

            var innerLoopBlock = Expression.Block(new[] { srcItmVarExp, destItmVarExp },
                new Expression[] { assignSourceItmFromProp, ifNotEndOfListExp, mapAndAddItemExp });

            var brk = Expression.Label();
            var loopExpression = Expression.Loop(
                Expression.IfThenElse(Expression.NotEqual(doMoveNextSrc, StaticExpressions.FalseConstant),
                    innerLoopBlock
                    , Expression.Break(brk))
                , brk);

            var blockExpression = Expression.Block(new[] { endOfListExp, enumeratorSrc, enumeratorDest }, new Expression[] { assignInitEndOfListExp, assignToEnumSrc, assignToEnumDest, loopExpression });
            return blockExpression;
        }

        internal Expression IfElseExpr(Expression srcItmVarExp,
            Expression destItmVarExp,
            Expression assignDestItmFromProp)
        {
            // TODO: Change name
            var mapExprForType = GetMemberMappingExpression(destItmVarExp, srcItmVarExp).Item2;

            return Expression.Block(new ParameterExpression[] { }, new[] { assignDestItmFromProp, mapExprForType });
        }

        internal LoopExpression CollectionLoopExpression(
            Type sourceType,
            Type destType,
            ParameterExpression destColl,
            ParameterExpression sourceColItmVariable,
            ParameterExpression destColItmVariable,
            BinaryExpression assignSourceItmFromProp,
            MethodCallExpression doMoveNext)
        {
            var mapExprForType = GetMemberMappingExpression(destColItmVariable, sourceColItmVariable).Item1;

            var addToNewColl = Expression.Call(destColl, "Add", null, destColItmVariable);

            var ifTrueBlock = Expression.Block(new[] { sourceColItmVariable, destColItmVariable }, new[] { assignSourceItmFromProp, mapExprForType, addToNewColl });

            var brk = Expression.Label();
            var loopExpression = Expression.Loop(
                Expression.IfThenElse(
                    Expression.NotEqual(doMoveNext, StaticExpressions.FalseConstant),
                    ifTrueBlock
                    , Expression.Break(brk))
                , brk);

            return loopExpression;
        }

        internal Expression ConvertCollection(Type destPropType,
            Type destList,
            Type destType,
            Expression destColl)
        {
            if (destPropType.IsArray)
            {
                return Expression.Call(destColl, destList.GetMethod("ToArray"));
            }
            else if (destPropType.IsGenericType)
            {
                if (typeof(IQueryable).IsAssignableFrom(destPropType))
                {
                    return Expression.Call(typeof(Queryable), "AsQueryable", new[] { destType }, destColl);
                }
                else
                {
                    Type collectionType = typeof(Collection<>).MakeGenericType(destType);

                    if (destPropType == collectionType)
                    {
                        return Expression.New(collectionType.GetConstructor(new Type[] { destList }), destColl);
                    }
                }
            }

            return destColl;
        }

        internal Tuple<Expression, Expression> GetMemberMappingExpression(Expression left, Expression right)
        {
            var nullCheckNestedMemberVisitor = new NullCheckNestedMemberVisitor();
            nullCheckNestedMemberVisitor.Visit(right);

            Type destNullableType = Nullable.GetUnderlyingType(left.Type);
            Type sourceNullableType = Nullable.GetUnderlyingType(right.Type);

            Type destType = destNullableType == null ? left.Type : destNullableType;
            Type sourceType = sourceNullableType == null ? right.Type : sourceNullableType;

            if (destType != sourceType)
            {
                var customMapExpression = GetCustomMapExpression(right.Type, left.Type);
                var customMapExpressionWithDest = GetCustomMapExpression(right.Type, left.Type, true);
                if (customMapExpression != null && customMapExpressionWithDest != null)
                {
                    var srcExp = Expression.Variable(right.Type,
                        string.Format("{0}Src", Guid.NewGuid().ToString("N")));
                    var assignSrcExp = Expression.Assign(srcExp, right);

                    var destExp = Expression.Variable(left.Type,
                        string.Format("{0}Dest", Guid.NewGuid().ToString("N")));
                    var assignDestExp = Expression.Assign(destExp, left);

                    var substituteParameterVisitor = new SubstituteParameterVisitor(srcExp, destExp);
                    var blockExpression = substituteParameterVisitor.Visit(customMapExpression) as BlockExpression;
                    var assignResultExp = Expression.Assign(left, destExp);
                    var resultBlockExp = Expression.Block(new[] { srcExp, destExp }, assignSrcExp, blockExpression, assignResultExp);
                    var resultBlockWithDestExp = Expression.Block(new[] { srcExp, destExp }, assignSrcExp, assignDestExp, blockExpression, assignResultExp);

                    var checkNullExp =
                        Expression.IfThenElse(Expression.Equal(right, Expression.Default(right.Type)),
                            Expression.Assign(left, Expression.Default(left.Type)), resultBlockExp);

                    var checkNullExpWithDest =
                        Expression.IfThenElse(Expression.Equal(right, Expression.Default(right.Type)),
                            Expression.Assign(left, Expression.Default(left.Type)), resultBlockWithDestExp);

                    var releaseExp = Expression.Block(new ParameterExpression[] { }, checkNullExp);
                    var releaseWithDestExp = Expression.Block(new ParameterExpression[] { }, checkNullExpWithDest);

                    return new Tuple<Expression, Expression>(releaseExp, releaseWithDestExp);
                }
                else if (typeof(IConvertible).IsAssignableFrom(destType) &&
                    typeof(IConvertible).IsAssignableFrom(sourceType))
                {
                    var assignExp = CreateConvertibleAssignExpression(left,
                        right,
                        left.Type,
                        sourceType,
                        destNullableType);

                    return new Tuple<Expression, Expression>(assignExp, assignExp);
                }
                else
                {
                    var mapComplexResult = GetDifferentTypeMemberMappingExpression(right, left);

                    return new Tuple<Expression, Expression>(nullCheckNestedMemberVisitor.CheckNullExpression != null
                            ? Expression.Condition(nullCheckNestedMemberVisitor.CheckNullExpression,
                                Expression.Assign(left, Expression.Default(left.Type)),
                                mapComplexResult.Item1)
                            : mapComplexResult.Item1,
                            nullCheckNestedMemberVisitor.CheckNullExpression != null
                            ? Expression.Condition(nullCheckNestedMemberVisitor.CheckNullExpression,
                                Expression.Assign(left, Expression.Default(left.Type)),
                                mapComplexResult.Item2)
                            : mapComplexResult.Item2);
                }
            }
            else
            {
                var binaryExpression = CreateAssignExpression(left,
                    right,
                    left.Type,
                    destNullableType,
                    sourceNullableType);

                var conditionalExpression = nullCheckNestedMemberVisitor.CheckNullExpression != null ? Expression.Condition(nullCheckNestedMemberVisitor.CheckNullExpression, Expression.Assign(left, Expression.Default(left.Type)), binaryExpression) : (Expression)binaryExpression;

                return new Tuple<Expression, Expression>(conditionalExpression, conditionalExpression);
            }
        }

        internal Tuple<Expression, Expression> GetDifferentTypeMemberMappingExpression(Expression callGetPropMethod, Expression callSetPropMethod)
        {
            Type sourceType = callGetPropMethod.Type;
            Type destType = callSetPropMethod.Type;

            var tCol =
                sourceType.GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                (sourceType.IsGenericType
                    && sourceType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? sourceType
                    : null);

            var tnCol = destType.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                        (destType.IsGenericType && destType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? destType
                            : null);

            var blockExpression = (tCol != null && tnCol != null)
                ? new Tuple<Expression, Expression>(MapCollection(tCol, tnCol, callGetPropMethod, callSetPropMethod), MapCollection2(tCol, tnCol, callGetPropMethod, callSetPropMethod))
                : new Tuple<Expression, Expression>(MapProperty(sourceType, destType, callGetPropMethod, callSetPropMethod), MapProperty2(sourceType, destType, callGetPropMethod, callSetPropMethod));


            var refSrcType = sourceType.IsClass;
            var destPropType = destType;
            if (refSrcType)
            {
                var resultExpression =
                    new Tuple<Expression, Expression>(
                        Expression.IfThenElse(Expression.Equal(callGetPropMethod, StaticExpressions.NullConstant),
                            Expression.Assign(callSetPropMethod, Expression.Default(destPropType)),
                            blockExpression.Item1),
                        Expression.IfThenElse(Expression.Equal(callGetPropMethod, StaticExpressions.NullConstant),
                            Expression.Assign(callSetPropMethod, Expression.Default(destPropType)),
                            blockExpression.Item2));
                return resultExpression;
            }
            return blockExpression;
        }

        private BlockExpression MapCollection(Type tCol, Type tnCol, Expression callGetPropMethod, Expression callSetPropMethod)
        {
            var sourceType = GetCollectionElementType(tCol);
            var destType = GetCollectionElementType(tnCol);
            var sourceVariable = Expression.Variable(callGetPropMethod.Type,
                string.Format("{0}Src", Guid.NewGuid().ToString().Replace("-", "_")));
            var assignSourceFromProp = Expression.Assign(sourceVariable, callGetPropMethod);

            var destList = typeof(List<>).MakeGenericType(destType);
            var destColl = Expression.Variable(destList, string.Format("{0}Dest", Guid.NewGuid().ToString().Replace("-", "_")));

            var newColl = Expression.New(destList);
            var destAssign = Expression.Assign(destColl, newColl);

            var closedEnumeratorSourceType = typeof(IEnumerator<>).MakeGenericType(sourceType);
            var closedEnumerableSourceType = typeof(IEnumerable<>).MakeGenericType(sourceType);
            var enumerator = Expression.Variable(closedEnumeratorSourceType,
                string.Format("{0}Enum", Guid.NewGuid().ToString().Replace("-", "_")));
            var assignToEnum = Expression.Assign(enumerator,
                Expression.Call(sourceVariable, closedEnumerableSourceType.GetMethod("GetEnumerator")));
            var doMoveNext = Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext"));

            var current = Expression.Property(enumerator, "Current");
            var sourceColItmVariable = Expression.Variable(sourceType,
                string.Format("{0}ItmSrc", Guid.NewGuid().ToString().Replace("-", "_")));
            var assignSourceItmFromProp = Expression.Assign(sourceColItmVariable, current);

            var destColItmVariable = Expression.Variable(destType,
                string.Format("{0}ItmDest", Guid.NewGuid().ToString().Replace("-", "_")));

            var loopExpression = CollectionLoopExpression(sourceType, destType, destColl, sourceColItmVariable, destColItmVariable,
                assignSourceItmFromProp, doMoveNext);

            Expression resultCollection = ConvertCollection(callSetPropMethod.Type, destList, destType, destColl);

            var assignResult = Expression.Assign(callSetPropMethod, resultCollection);

            var parameters = new List<ParameterExpression> { sourceVariable, destColl, enumerator };
            var expressions = new List<Expression>
            {
                assignSourceFromProp,
                destAssign,
                assignToEnum,
                loopExpression,
                assignResult
            };

            var blockExpression = Expression.Block(parameters, expressions);

            var checkSrcForNullExp =
                Expression.IfThenElse(Expression.Equal(callGetPropMethod, StaticExpressions.NullConstant),
                    Expression.Assign(callSetPropMethod, Expression.Default(callSetPropMethod.Type)), blockExpression);
            var blockResultExp = Expression.Block(new ParameterExpression[] { }, new Expression[] { checkSrcForNullExp });

            return blockResultExp;
        }

        private BlockExpression MapCollection2(Type tCol, Type tnCol, Expression callGetPropMethod, Expression callSetPropMethod)
        {
            var sourceType = GetCollectionElementType(tCol);
            var destType = GetCollectionElementType(tnCol);
            var sourceVariable = Expression.Variable(callGetPropMethod.Type,
                string.Format("{0}Src", Guid.NewGuid().ToString().Replace("-", "_")));
            var assignSourceVarExp = Expression.Assign(sourceVariable, callGetPropMethod);

            var srcCount = Expression.Call(typeof(Enumerable), "Count", new[] { sourceType }, sourceVariable);
            var destCount = Expression.Call(typeof(Enumerable), "Count", new[] { destType }, callSetPropMethod);

            var conditionToCreateList = Expression.NotEqual(srcCount, destCount);
            var notNullCondition = Expression.IfThenElse(conditionToCreateList,
                MapCollectionNotCountEquals(callGetPropMethod.Type, callSetPropMethod.Type, callGetPropMethod,
                    callSetPropMethod),
                MapCollectionCountEquals(tCol, tnCol, callGetPropMethod, callSetPropMethod));

            var result = Expression.IfThenElse(Expression.NotEqual(callSetPropMethod, StaticExpressions.NullConstant), notNullCondition,
                MapCollection(tCol, tnCol, callGetPropMethod, callSetPropMethod));

            var expressions = new List<Expression> { assignSourceVarExp, result };

            var resultExpression = Expression.Block(new[] { sourceVariable }, expressions);

            var checkSrcForNullExp =
                Expression.IfThenElse(Expression.Equal(callGetPropMethod, StaticExpressions.NullConstant),
                    Expression.Assign(callSetPropMethod, Expression.Default(callSetPropMethod.Type)), resultExpression);
            var block = Expression.Block(new ParameterExpression[] { }, new Expression[] { checkSrcForNullExp });

            return block;
        }

        private static Expression CreateAssignExpression(Expression setMethod, Expression getMethod, Type setType, Type setNullableType, Type getNullableType)
        {
            Expression left = setMethod;
            Expression right = getMethod;

            if (setNullableType == null && getNullableType != null)
            {
                // Nullable to non nullable map
                right = Expression.Call(getMethod, "GetValueOrDefault", Type.EmptyTypes);
            }
            else if (setNullableType != null && getNullableType == null)
            {
                // Non nullable to nullable  map
                right = Expression.Convert(getMethod, setType);
            }

            return Expression.Assign(left, right);
        }

        private BlockExpression MapProperty(Type sourceType, Type destType, Expression callGetPropMethod, Expression callSetPropMethod)
        {
            var sourceVariable = Expression.Variable(sourceType,
                string.Format("{0}_{1}Src", sourceType.Name, Guid.NewGuid().ToString().Replace("-", "_")));
            var assignSourceFromProp = Expression.Assign(sourceVariable, callGetPropMethod);
            var mapExprForType = GetMapExpressions(sourceType, destType);
            var destVariable = Expression.Variable(destType,
                string.Format("{0}_{1}Dest", destType.Name,
                    Guid.NewGuid().ToString().Replace("-", "_")));
            var blockForSubstitution = Expression.Block(mapExprForType);
            var substBlock =
                new SubstituteParameterVisitor(sourceVariable, destVariable).Visit(blockForSubstitution) as
                    BlockExpression;
            var resultMapExprForType = substBlock.Expressions;

            var assignExp = Expression.Assign(callSetPropMethod, destVariable);

            var expressions = new List<Expression> { assignSourceFromProp };
            expressions.AddRange(resultMapExprForType);
            expressions.Add(assignExp);

            var parameterExpressions = new List<ParameterExpression> { sourceVariable, destVariable };
            var blockExpression = Expression.Block(parameterExpressions, expressions);

            return blockExpression;
        }

        private BlockExpression MapProperty2(Type sourceType, Type destType, Expression callGetPropMethod, Expression callSetPropMethod)
        {
            var sourceVariable = Expression.Variable(sourceType,
                string.Format("{0}_{1}Src", sourceType.Name, Guid.NewGuid().ToString().Replace("-", "_")));

            var assignSourceFromProp = Expression.Assign(sourceVariable, callGetPropMethod);
            var mapExprForType = new List<Expression>(GetMapExpressions(sourceType, destType, true));
            var destVariable = Expression.Variable(destType,
                string.Format("{0}_{1}Dest", destType.Name,
                    Guid.NewGuid().ToString().Replace("-", "_")));
            var assignDestFromProp = Expression.Assign(destVariable, callSetPropMethod);

            var ifDestNull = destType.IsPrimitive || destType.IsEnum ? (Expression)StaticExpressions.FalseConstant : Expression.Equal(callSetPropMethod, StaticExpressions.NullConstant);

            var newDestInstanceExp = mapExprForType[0] as BinaryExpression;
            if (newDestInstanceExp != null)
            {
                mapExprForType.RemoveAt(0);

                var destVar = newDestInstanceExp.Left as ParameterExpression;

                var assignExistingDestExp = Expression.Assign(destVar, callSetPropMethod);

                var destCondition = Expression.IfThenElse(ifDestNull, newDestInstanceExp, assignExistingDestExp);
                mapExprForType.Insert(0, destCondition);
            }

            var blockForSubstitution = Expression.Block(mapExprForType);
            var substBlock =
                new SubstituteParameterVisitor(sourceVariable, destVariable).Visit(blockForSubstitution) as
                    BlockExpression;
            var resultMapExprForType = substBlock.Expressions;

            var assignExp = Expression.Assign(callSetPropMethod, destVariable);

            var expressions = new List<Expression>();
            expressions.Add(assignSourceFromProp);
            expressions.Add(assignDestFromProp);
            expressions.AddRange(resultMapExprForType);
            expressions.Add(assignExp);

            var parameterExpressions = new List<ParameterExpression> { sourceVariable, destVariable };
            var blockExpression = Expression.Block(parameterExpressions, expressions);

            return blockExpression;
        }

        private static Expression CreateConvertibleAssignExpression(Expression setMethod, Expression getMethod, Type setType, Type getType, Type setNullableType)
        {
            Expression left = setMethod;
            Expression right = getMethod;

            if ((setNullableType ?? setType).IsEnum && (getType == typeof(string)))
            {
                return Expression.IfThen(
                    Expression.NotEqual(getMethod, StaticExpressions.NullConstant),
                        Expression.Assign(left,
                            Expression.Convert(
                                Expression.Call(typeof(Enum).GetMethod("Parse", new Type[] { typeof(Type), typeof(string), typeof(bool) }), Expression.Constant(setNullableType ?? setType), right, Expression.Constant(true)),
                                setType)));
            }
            else
            {
                return Expression.IfThen(
                    Expression.NotEqual(Expression.Convert(getMethod, typeof(object)), StaticExpressions.NullConstant),
                        Expression.Assign(left,
                            Expression.Convert(
                                Expression.Call(typeof(Convert).GetMethod("ChangeType", new Type[] { typeof(object), typeof(Type) }), Expression.Convert(right, typeof(object)), Expression.Constant(setNullableType ?? setType)),
                                setType)));
            }
        }

        private static int CalculateCacheKey(Type source, Type dest)
        {
            var destHashCode = dest.GetHashCode();
            return source.GetHashCode() ^ ((destHashCode << 16) | (destHashCode >> 16));
        }

        #endregion
    }
}

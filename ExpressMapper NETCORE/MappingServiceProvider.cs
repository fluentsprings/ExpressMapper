using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressMapper
{
    public sealed class MappingServiceProvider : IMappingServiceProvider
    {
        private readonly object _lock = new object();

        public Dictionary<long, Func<ICustomTypeMapper>> CustomMappers { get; set; }
        public Dictionary<int, IList<long>> CustomMappingsBySource { get; set; }
        private readonly Dictionary<long, Func<object, object, object>> _customTypeMapperCache = new Dictionary<long, Func<object, object, object>>();
        private readonly List<long> _nonGenericCollectionMappingCache = new List<long>();

        private static readonly Type GenericEnumerableType = typeof(IEnumerable<>);
        private readonly IList<IMappingService> _mappingServices;

        private IMappingService SourceService
        {
            get { return _mappingServices.First(m => !m.DestinationSupport); }
        }

        private IMappingService DestinationService
        {
            get { return _mappingServices.First(m => m.DestinationSupport); }
        }

        public MappingServiceProvider()
        {
            // todo : make it via internal DependencyResolver - IOC
            _mappingServices = new List<IMappingService>
            {
                new SourceMappingService(this),
                new DestinationMappingService(this)
            };
            CustomMappers = new Dictionary<long, Func<ICustomTypeMapper>>();
            CustomMappingsBySource = new Dictionary<int, IList<long>>();
        }

        public IQueryable<TN> Project<T, TN>(IQueryable<T> source)
        {
            var srcType = typeof(T);
            var destType = typeof(TN);
            var cacheKey = CalculateCacheKey(srcType, destType);

            if (!SourceService.TypeMappers.ContainsKey(cacheKey)) return null;

            var typeMapper = SourceService.TypeMappers[cacheKey];
            var mapper = typeMapper as ITypeMapper<T, TN>;
            if (mapper.QueryableExpression == null)
            {
                mapper.Compile(CompilationTypes.Source);
            }
            return source.Select(mapper.QueryableExpression);
        }

        public bool CaseSensetiveMemberMap { get; set; }

        public IMemberConfiguration<T, TN> Register<T, TN>()
        {
            return RegisterInternal<T, TN>();
        }

        private IMemberConfiguration<T, TN> Register<T, TN>(T src, TN dest)
        {
            return RegisterInternal<T, TN>();
        }

        public IMemberConfiguration<T, TN> Register<T, TN>(IMemberConfigParameters baseType)
        {
            var memberConfiguration = Register<T, TN>();
            var src = typeof(T);
            var dest = typeof(TN);
            var cacheKey = CalculateCacheKey(src, dest);

            if (SourceService.TypeMappers.ContainsKey(cacheKey) &&
                DestinationService.TypeMappers.ContainsKey(cacheKey))
            {
                var typeMapper = SourceService.TypeMappers[cacheKey] as ITypeMapper<T, TN>;
                typeMapper?.ImportMemberConfigParameters(baseType);
            }
            return memberConfiguration;
        }

        public IMemberConfiguration<T, TN> Register<T, TN>(bool memberCaseInsensitive)
        {
            return RegisterInternal<T, TN>(memberCaseInsensitive);
        }

        #region flatten code

        /// <summary>
        /// This returns true if ExpressMapper already contains a map between the source and destination types
        /// </summary>
        /// <returns></returns>
        public bool MapExists(Type sourceType, Type destinationType)
        {
            var cacheKey = CalculateCacheKey(sourceType, destinationType);

            return SourceService.TypeMappers.ContainsKey(cacheKey) &&
                   DestinationService.TypeMappers.ContainsKey(cacheKey);
        }

        #endregion

        private IMemberConfiguration<T, TN> RegisterInternal<T, TN>(bool memberCaseInsensitive = true)
        {
            lock (_lock)
            {
                var src = typeof(T);
                var dest = typeof(TN);
                var cacheKey = CalculateCacheKey(src, dest);

                if (SourceService.TypeMappers.ContainsKey(cacheKey) &&
                    DestinationService.TypeMappers.ContainsKey(cacheKey))
                {
                    var typeMapper = SourceService.TypeMappers[cacheKey] as ITypeMapper<T, TN>;
                    return typeMapper?.MemberConfiguration;
                }

                if (src.GetInfo().GetInterfaces().Any(t => t.Name.Contains(typeof(IEnumerable).Name)) &&
                    dest.GetInfo().GetInterfaces().Any(t => t.Name.Contains(typeof(IEnumerable).Name)))
                {
                    throw new InvalidOperationException(
                        $"It is invalid to register mapping for collection types from {src.FullName} to {dest.FullName}, please use just class registration mapping and your collections will be implicitly processed. In case you want to include some custom collection mapping please use: Mapper.RegisterCustom.");
                }

                var sourceClassMapper = new SourceTypeMapper<T, TN>(SourceService, this);
                var destinationClassMapper = new DestinationTypeMapper<T, TN>(DestinationService, this);

                SourceService.TypeMappers[cacheKey] = sourceClassMapper;
                DestinationService.TypeMappers[cacheKey] = destinationClassMapper;
                var memberConfiguration = new MemberConfiguration<T, TN>(
                    new ITypeMapper<T, TN>[] {sourceClassMapper, destinationClassMapper}, this);
                sourceClassMapper.MemberConfiguration = memberConfiguration;
                destinationClassMapper.MemberConfiguration = memberConfiguration;

                if (!CustomMappingsBySource.ContainsKey(src.GetHashCode()))
                {
                    CustomMappingsBySource[src.GetHashCode()] = new List<long>();
                }
                if (!CustomMappingsBySource[src.GetHashCode()].Contains(cacheKey))
                {
                    CustomMappingsBySource[src.GetHashCode()].Add(cacheKey);
                }

                return memberConfiguration;
            }
        }

        public void Compile()
        {
            lock (_lock)
            {
                foreach (var mappingService in _mappingServices)
                {
                    mappingService.Compile(CompilationTypes.Source | CompilationTypes.Destination);
                }
            }
        }

        public void Compile(CompilationTypes compilationType)
        {
            lock (_lock)
            {
                foreach (var mappingService in _mappingServices)
                {
                    mappingService.Compile(compilationType);
                }
            }
        }

        public void PrecompileCollection<T, TN>()
        {
            lock (_lock)
            {
                foreach (var mappingService in _mappingServices)
                {
                    mappingService.PrecompileCollection<T, TN>();
                }
            }
        }

        public void PrecompileCollection<T, TN>(CompilationTypes compilationType)
        {
            lock (_lock)
            {
                foreach (var mappingService in _mappingServices)
                {
                    if ((CompilationTypes.Source & compilationType) == CompilationTypes.Source)
                    {
                        if (!mappingService.DestinationSupport)
                        {
                            mappingService.PrecompileCollection<T, TN>();
                        }
                    }
                    if ((CompilationTypes.Destination & compilationType) == CompilationTypes.Destination)
                    {
                        if (mappingService.DestinationSupport)
                        {
                            mappingService.PrecompileCollection<T, TN>();
                        }
                    }
                }
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                foreach (var mappingService in _mappingServices)
                {
                    mappingService.TypeMappers.Clear();
                }

                CustomMappers.Clear();

                foreach (var mappingService in _mappingServices)
                {
                    mappingService.Reset();
                }
                CaseSensetiveMemberMap = false;
            }
        }

        public void RegisterCustom<T, TN>(Func<T, TN> mapFunc)
        {
            lock (_lock)
            {
                var src = typeof(T);
                var dest = typeof(TN);
                var cacheKey = CalculateCacheKey(src, dest);

                if (CustomMappers.ContainsKey(cacheKey))
                {
                    throw new InvalidOperationException(
                        $"Mapping from {src.FullName} to {dest.FullName} is already registered");
                }

                var delegateMapperType = typeof(DelegateCustomTypeMapper<,>).MakeGenericType(src, dest);
                var newExpression =
                    Expression.New(
                        delegateMapperType.GetInfo().GetConstructor(new Type[] { typeof(Func<,>).MakeGenericType(src, dest) }),
                        Expression.Constant(mapFunc));
                var newLambda = Expression.Lambda<Func<ICustomTypeMapper<T, TN>>>(newExpression);
                var compile = newLambda.Compile();
                CustomMappers.Add(cacheKey, compile);
            }
        }

        public void RegisterCustom<T, TN, TMapper>() where TMapper : ICustomTypeMapper<T, TN>
        {
            lock (_lock)
            {
                var src = typeof(T);
                var dest = typeof(TN);
                var cacheKey = CalculateCacheKey(src, dest);

                if (CustomMappers.ContainsKey(cacheKey))
                {
                    throw new InvalidOperationException(
                        $"Mapping from {src.FullName} to {dest.FullName} is already registered");
                }

                var newExpression = Expression.New(typeof(TMapper));
                var newLambda = Expression.Lambda<Func<ICustomTypeMapper<T, TN>>>(newExpression);
                var compile = newLambda.Compile();
                CustomMappers[cacheKey] = compile;
            }
        }

        public TN Map<T, TN>(T src)
        {
            if (src == null || src.GetType() == typeof(T))
            {
                return MapInternal<T, TN>(src);
            }
            return (TN)Map(typeof(T), typeof(TN), src);
        }

        public TN Map<T, TN>(T src, TN dest)
        {
            if ((src == null || src.GetType() == typeof(T)) && (dest == null || dest.GetType() == typeof(TN)))
            {
                return MapInternal<T, TN>(src, dest);
            }
            return (TN)Map(typeof(T), typeof(TN), src, dest);
        }

        private TN MapInternal<T, TN>(T src, TN dest = default(TN), bool dynamicTrial = false)
        {
            var srcType = typeof(T);
            var destType = typeof(TN);
            var cacheKey = CalculateCacheKey(srcType, destType);

            if (CustomMappers.ContainsKey(cacheKey))
            {
                var customTypeMapper = CustomMappers[cacheKey];
                var typeMapper = customTypeMapper() as ICustomTypeMapper<T, TN>;
                var context = new DefaultMappingContext<T, TN> { Source = src, Destination = dest };
                return typeMapper.Map(context);
            }

            var mappingService = EqualityComparer<TN>.Default.Equals(dest, default(TN)) ? SourceService : DestinationService;

            if (mappingService.TypeMappers.ContainsKey(cacheKey))
            {
                if (EqualityComparer<T>.Default.Equals(src, default(T)))
                {
                    return default(TN);
                }

                var mapper = mappingService.TypeMappers[cacheKey] as ITypeMapper<T, TN>;
                return mapper != null
                    ? mapper.MapTo(src, dest)
                    : default(TN);
            }

            var tCol =
                typeof(T).GetInfo().GetInterfaces()
                    .FirstOrDefault(t => t.GetInfo().IsGenericType && t.GetGenericTypeDefinition() == GenericEnumerableType) ??
                (typeof(T).GetInfo().IsGenericType
                 && typeof(T).GetInfo().GetInterfaces().Any(t => t == typeof(IEnumerable)) ? typeof(T)
                    : null);

            var tnCol = typeof(TN).GetInfo().GetInterfaces()
                            .FirstOrDefault(t => t.GetInfo().IsGenericType && t.GetGenericTypeDefinition() == GenericEnumerableType) ??
                        (typeof(TN).GetInfo().IsGenericType && typeof(TN).GetInfo().GetInterfaces().Any(t => t == typeof(IEnumerable)) ? typeof(TN)
                            : null);

            if ((tCol == null || tnCol == null))
            {
                if (dynamicTrial)
                {
                    throw new MapNotImplementedException(
                        $"There is no mapping has been found. Source Type: {srcType.FullName}, Destination Type: {destType.FullName}");
                }
                Register<T, TN>();
                return MapInternal(src, dest, true);
            }

            if (EqualityComparer<TN>.Default.Equals(dest, default(TN)))
            {
                PrecompileCollection<T, TN>(CompilationTypes.Source);
            }
            else
            {
                PrecompileCollection<T, TN>();
            }

            // todo: make same signature in both compiled funcs with destination
            var result = (TN)(((EqualityComparer<TN>.Default.Equals(dest, default(TN)))
                ? SourceService.MapCollection(cacheKey).DynamicInvoke(src)
                : DestinationService.MapCollection(cacheKey).DynamicInvoke(src, dest)));
            return result;
        }

        public TN Map<T, TN>(T src, ICustomTypeMapper<T, TN> customMapper)
        {
            return customMapper.Map(new DefaultMappingContext<T, TN> { Source = src, Destination = default(TN) });
        }

        public TN Map<T, TN>(T src, ICustomTypeMapper<T, TN> customMapper, TN dest)
        {
            return customMapper.Map(new DefaultMappingContext<T, TN> { Source = src, Destination = dest });
        }

        public object Map(Type srcType, Type dstType, object src)
        {
            return MapNonGenericInternal(srcType, dstType, src, null);
        }

        public object Map(Type srcType, Type dstType, object src, object dest)
        {
            return MapNonGenericInternal(srcType, dstType, src, dest);
        }

        private object MapNonGenericInternal(Type srcType, Type dstType, object src, object dest = null, bool dynamicTrial = false)
        {
            if (src == null)
            {
                return null;
            }

            var cacheKey = CalculateCacheKey(srcType, dstType);
            if (CustomMappers.ContainsKey(cacheKey))
            {
                var customTypeMapper = CustomMappers[cacheKey];
                var typeMapper = customTypeMapper();
                if (!_customTypeMapperCache.ContainsKey(cacheKey))
                {
                    CompileNonGenericCustomTypeMapper(srcType, dstType, typeMapper, cacheKey);
                }
                return _customTypeMapperCache[cacheKey](src, dest);
            }

            ITypeMapper mapper = null;
            var actualSrcType = src.GetType();
            if (srcType != actualSrcType && actualSrcType.GetInfo().IsAssignableFrom(srcType))
                throw new InvalidCastException($"Your source object instance type '{actualSrcType.FullName}' is not assignable from source type you specified '{srcType}'.");

            var srcHash = actualSrcType.GetHashCode();

            if (dest != null)
            {
                var actualDstType = dest.GetType();
                if (dstType != actualDstType && actualDstType.GetInfo().IsAssignableFrom(dstType))
                    throw new InvalidCastException($"Your destination object instance type '{actualSrcType.FullName}' is not assignable from destination type you specified '{srcType}'.");

                if (CustomMappingsBySource.ContainsKey(srcHash))
                {
                    var mappings = CustomMappingsBySource[srcHash];

                    mapper =
                        mappings.Where(m => DestinationService.TypeMappers.ContainsKey(m))
                            .Select(m => DestinationService.TypeMappers[m])
                            .FirstOrDefault(tm => tm.DestinationType == actualDstType);
                }
            }
            else
            {
                if (CustomMappingsBySource.ContainsKey(srcHash))
                {
                    var mappings = CustomMappingsBySource[srcHash];
                    var typeMappers =
                        mappings.Where(m => SourceService.TypeMappers.ContainsKey(m))
                            .Select(m => SourceService.TypeMappers[m])
                            .Where(m => dstType.GetInfo().IsAssignableFrom(m.DestinationType))
                            .ToList();
                    if (typeMappers.Count > 1)
                    {
                        if (typeMappers.All(tm => tm.DestinationType != dstType))
                        {
                            throw new AmbiguousMatchException(
                                $"Source '{actualSrcType.FullName}' has more than one destination types' mappings");
                        }
                        mapper = typeMappers.FirstOrDefault(tm => tm.DestinationType == dstType);
                    }
                    else
                    {
                        mapper = typeMappers.First();
                    }
                }
            }

            var mappingService = dest == null ? SourceService : DestinationService;
            if (mapper != null)
            {
                var nonGenericMapFunc = mapper.GetNonGenericMapFunc();
                return nonGenericMapFunc(src, dest);
            }

            if (mappingService.TypeMappers.ContainsKey(cacheKey))
            {
                mapper = mappingService.TypeMappers[cacheKey];
                var nonGenericMapFunc = mapper.GetNonGenericMapFunc();
                return nonGenericMapFunc(src, dest);
            }

            var tCol =
                srcType.GetInfo().GetInterfaces()
                    .FirstOrDefault(t => t.GetInfo().IsGenericType && t.GetGenericTypeDefinition() == GenericEnumerableType) ??
                (srcType.GetInfo().IsGenericType
                 && srcType.GetInfo().GetInterfaces().Any(t => t == typeof(IEnumerable))
                    ? srcType
                    : null);

            var tnCol = dstType.GetInfo().GetInterfaces()
                            .FirstOrDefault(t => t.GetInfo().IsGenericType && t.GetGenericTypeDefinition() == GenericEnumerableType) ??
                        (dstType.GetInfo().IsGenericType && dstType.GetInfo().GetInterfaces().Any(t => t == typeof(IEnumerable))
                            ? dstType
                            : null);

            if (tCol != null && tnCol != null)
            {
                CompileNonGenericCollectionMapping(srcType, dstType);
                // todo: make same signature in both compiled funcs with destination
                var result = (dest == null
                    ? _mappingServices.First(m => !m.DestinationSupport).MapCollection(cacheKey).DynamicInvoke(src)
                    : _mappingServices.First(m => m.DestinationSupport).MapCollection(cacheKey).DynamicInvoke(src, dest));
                return result;
            }

            if (dynamicTrial)
            {
                throw new MapNotImplementedException(
                    $"There is no mapping has been found. Source Type: {srcType.FullName}, Destination Type: {dstType.FullName}");
            }
            dynamic source = src;
            dynamic destination = dest;
            Register(source, destination);
            return MapNonGenericInternal(srcType, dstType, src, dest, true);
        }

        #region Helper methods

        private void CompileNonGenericCollectionMapping(Type srcType, Type dstType)
        {
            var cacheKey = CalculateCacheKey(srcType, dstType);
            if (_nonGenericCollectionMappingCache.Contains(cacheKey)) return;

            var methodInfo = GetType().GetInfo().GetMethod("PrecompileCollection", new Type[] { });
            var makeGenericMethod = methodInfo.MakeGenericMethod(srcType, dstType);
            var methodCallExpression = Expression.Call(Expression.Constant(this), makeGenericMethod);
            var expression = Expression.Lambda<Action>(methodCallExpression);
            var action = expression.Compile();
            action();
        }

        private void CompileNonGenericCustomTypeMapper(Type srcType, Type dstType, ICustomTypeMapper typeMapper, long cacheKey)
        {
            var sourceExpression = Expression.Parameter(typeof(object), "src");
            var destinationExpression = Expression.Parameter(typeof(object), "dst");
            var srcConverted = Expression.Convert(sourceExpression, srcType);
            var srcTypedExp = Expression.Variable(srcType, "srcTyped");
            var srcAssigned = Expression.Assign(srcTypedExp, srcConverted);

            var dstConverted = Expression.Convert(destinationExpression, dstType);
            var dstTypedExp = Expression.Variable(dstType, "dstTyped");
            var dstAssigned = Expression.Assign(dstTypedExp,
                Expression.Condition(Expression.Equal(destinationExpression, Expression.Constant(null)),
                    Expression.Default(dstType), dstConverted));

            var customGenericType = typeof(ICustomTypeMapper<,>).MakeGenericType(srcType, dstType);
            var castToCustomGeneric = Expression.Convert(Expression.Constant(typeMapper, typeof(ICustomTypeMapper)),
                customGenericType);
            var genVariable = Expression.Variable(customGenericType);
            var assignExp = Expression.Assign(genVariable, castToCustomGeneric);
            var methodInfo = customGenericType.GetInfo().GetMethod("Map");
            var genericMappingContextType = typeof(DefaultMappingContext<,>).MakeGenericType(srcType, dstType);
            var newMappingContextExp = Expression.New(genericMappingContextType);

            var contextVarExp = Expression.Variable(genericMappingContextType, $"context{Guid.NewGuid()}");
            var assignContextExp = Expression.Assign(contextVarExp, newMappingContextExp);

            var sourceExp = Expression.Property(contextVarExp, "Source");
            var sourceAssignedExp = Expression.Assign(sourceExp, srcTypedExp);

            var destExp = Expression.Property(contextVarExp, "Destination");
            var destAssignedExp = Expression.Assign(destExp, dstTypedExp);


            //var destinationAssignedExp = Expression.Assign(destinationExpression, dstTypedExp);

            var mapCall = Expression.Call(genVariable, methodInfo, contextVarExp);
            var resultVarExp = Expression.Variable(typeof(object), "result");
            var resultAssignExp = Expression.Assign(resultVarExp, Expression.Convert(mapCall, typeof(object)));

            var blockExpression = Expression.Block(new[] { srcTypedExp, dstTypedExp, genVariable, contextVarExp, resultVarExp },
                srcAssigned, dstAssigned, assignExp, assignContextExp, sourceAssignedExp, destAssignedExp, /*destinationAssignedExp,*/ resultAssignExp, resultVarExp);

            var lambda = Expression.Lambda<Func<object, object, object>>(blockExpression, sourceExpression, destinationExpression);
            _customTypeMapperCache[cacheKey] = lambda.Compile();
        }

        internal static Type GetCollectionElementType(Type type)
        {
            return type.IsArray ? type.GetElementType() : type.GetInfo().GetGenericArguments()[0];
        }

        public long CalculateCacheKey(Type source, Type dest)
        {
            var destHashCode = (uint)dest.GetHashCode();
            var sourceHashCode = (uint)source.GetHashCode();

            return (long)((((ulong)sourceHashCode) << 32) | ((ulong)destHashCode));
        }

        #endregion
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using ExpressMapper.Extensions;

namespace ExpressMapper
{
    internal abstract class MappingServiceBase
    {
        protected readonly Dictionary<int, BlockExpression> CustomTypeMapperExpCache = new Dictionary<int, BlockExpression>();
        public IDictionary<int, ITypeMapper> TypeMappers { get; set; }
        protected IMappingServiceProvider MappingServiceProvider { get; private set; }
        internal MappingServiceBase(IMappingServiceProvider mappingServiceProvider)
        {
            MappingServiceProvider = mappingServiceProvider;
            TypeMappers = new Dictionary<int, ITypeMapper>();
        }

        protected static readonly Type GenericEnumerableType = typeof(IEnumerable<>);
        public abstract IDictionary<int, MulticastDelegate> CollectionMappers { get; }
        public abstract void PrecompileCollection<T, TN>();
        public abstract bool DestinationSupport { get; }
        public abstract MulticastDelegate MapCollection(int cacheKey);
        public void Reset()
        {
            CollectionMappers.Clear();
            TypeMappers.Clear();
        }

        public void Compile()
        {
            var typeMappers = new Dictionary<int, ITypeMapper>(TypeMappers);
            foreach (var typeMapper in typeMappers)
            {
                typeMapper.Value.Compile();
            }
        }

        protected BlockExpression GetCustomMapExpression(Type src, Type dest)
        {
            var cacheKey = MappingServiceProvider.CalculateCacheKey(src, dest);
            if (!MappingServiceProvider.CustomMappers.ContainsKey(cacheKey)) return null;
            CompileGenericCustomTypeMapper(src, dest, MappingServiceProvider.CustomMappers[cacheKey](), cacheKey);
            return CustomTypeMapperExpCache[cacheKey];
        }

        protected Tuple<List<Expression>, ParameterExpression, ParameterExpression> GetMapExpressions(Type src, Type dest)
        {
            var cacheKey = MappingServiceProvider.CalculateCacheKey(src, dest);
            if (TypeMappers.ContainsKey(cacheKey))
            {
                return TypeMappers[cacheKey].GetMapExpressions();
            }
            
            dynamic srcInst = Activator.CreateInstance(src);
            dynamic destInst = Activator.CreateInstance(dest);
            RegisterDynamic(srcInst, destInst);
            if (TypeMappers.ContainsKey(cacheKey))
            {
                return TypeMappers[cacheKey].GetMapExpressions();
            }

            throw new MapNotImplementedException(string.Format("There is no mapping has bee found. Source Type: {0}, Destination Type: {1}", src.FullName, dest.FullName));
        }

        private static void RegisterDynamic<T, TN>(T src, TN dest)
        {
            Mapper.Register<T, TN>();
        }

        protected void CompileGenericCustomTypeMapper(Type srcType, Type dstType, ICustomTypeMapper typeMapper, int cacheKey)
        {
            if (CustomTypeMapperExpCache.ContainsKey(cacheKey)) return;

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
            //var resultVarExp = Expression.Variable(dstType, "result");
            //var resultAssignExp = Expression.Assign(resultVarExp, mapCall);
            var resultAssignExp = Expression.Assign(dstTypedExp, mapCall);

            var blockExpression = Expression.Block(new[] { genVariable, contextVarExp }, assignExp, assignContextExp, sourceAssignedExp, destAssignedExp, resultAssignExp);

            CustomTypeMapperExpCache[cacheKey] = Expression.Block(new ParameterExpression[] { }, blockExpression);
        }

        protected virtual bool ComplexMapCondition(Type src, Type dst)
        {
            return src != dst;
        }

        public Expression GetMemberMappingExpression(Expression left, Expression right)
        {
            var nullCheckNestedMemberVisitor = new NullCheckNestedMemberVisitor();
            nullCheckNestedMemberVisitor.Visit(right);

            var destNullableType = Nullable.GetUnderlyingType(left.Type);
            var sourceNullableType = Nullable.GetUnderlyingType(right.Type);

            var destType = destNullableType ?? left.Type;
            var sourceType = sourceNullableType ?? right.Type;

            if (ComplexMapCondition(sourceType, destType))
            {
                var customMapExpression = GetCustomMapExpression(right.Type, left.Type);
                if (customMapExpression != null)
                {
                    var srcExp = Expression.Variable(right.Type,
                        string.Format("{0}Src", Guid.NewGuid().ToString("N")));
                    var assignSrcExp = Expression.Assign(srcExp, right);

                    var destExp = Expression.Variable(left.Type,
                        string.Format("{0}Dst", Guid.NewGuid().ToString("N")));
                    var assignDestExp = Expression.Assign(destExp, left);

                    // try precise substitute visitor
                    var substituteParameterVisitor =
                        new PreciseSubstituteParameterVisitor(
                            new KeyValuePair<ParameterExpression, ParameterExpression>(
                                Expression.Variable(right.Type, "srcTyped"), srcExp),
                            new KeyValuePair<ParameterExpression, ParameterExpression>(
                                Expression.Variable(left.Type, "dstTyped"), destExp));

                    var blockExpression = substituteParameterVisitor.Visit(customMapExpression) as BlockExpression;

                    var assignResultExp = Expression.Assign(left, destExp);
                    var resultBlockExp = Expression.Block(new[] { srcExp, destExp }, assignSrcExp, assignDestExp, blockExpression, assignResultExp);

                    var checkNullExp =
                        Expression.IfThenElse(Expression.Equal(right, Expression.Default(right.Type)),
                            Expression.Assign(left, Expression.Default(left.Type)), resultBlockExp);

                    var releaseExp = Expression.Block(new ParameterExpression[] { }, checkNullExp);

                    return releaseExp;
                }

                if (typeof(IConvertible).IsAssignableFrom(destType) &&
                    typeof(IConvertible).IsAssignableFrom(sourceType))
                {
                    var assignExp = CreateConvertibleAssignExpression(left,
                        right,
                        left.Type,
                        sourceType,
                        destNullableType);

                    return assignExp;
                }
                var mapComplexResult = GetDifferentTypeMemberMappingExpression(right, left);

                return nullCheckNestedMemberVisitor.CheckNullExpression != null
                    ? Expression.Condition(nullCheckNestedMemberVisitor.CheckNullExpression,
                        Expression.Assign(left, Expression.Default(left.Type)),
                        mapComplexResult)
                    : mapComplexResult;
            }
            var binaryExpression = CreateAssignExpression(left,
                right,
                left.Type,
                destNullableType,
                sourceNullableType);

            var conditionalExpression = nullCheckNestedMemberVisitor.CheckNullExpression != null ? Expression.Condition(nullCheckNestedMemberVisitor.CheckNullExpression, Expression.Assign(left, Expression.Default(left.Type)), binaryExpression) : (Expression)binaryExpression;

            return conditionalExpression;
        }

        private static Expression CreateAssignExpression(Expression setMethod, Expression getMethod, Type setType, Type setNullableType, Type getNullableType)
        {
            var left = setMethod;
            var right = getMethod;

            if (setNullableType == null && getNullableType != null)
            {
                // Nullable to non nullable map
                // Type.EmptyTypes is not being used - PCL support
                right = Expression.Call(getMethod, "GetValueOrDefault", /*Type.EmptyTypes*/null);
            }
            else if (setNullableType != null && getNullableType == null)
            {
                // Non nullable to nullable  map
                right = Expression.Convert(getMethod, setType);
            }

            return Expression.Assign(left, right);
        }

        private static Expression CreateConvertibleAssignExpression(Expression setMethod, Expression getMethod, Type setType, Type getType, Type setNullableType)
        {
            var left = setMethod;
            var right = getMethod;

            if ((setNullableType ?? setType).IsEnum && (getType == typeof(string)))
            {
                return Expression.IfThen(
                    Expression.NotEqual(getMethod, StaticExpressions.NullConstant),
                        Expression.Assign(left,
                            Expression.Convert(
                                Expression.Call(typeof(Enum).GetMethod("Parse", new Type[] { typeof(Type), typeof(string), typeof(bool) }), Expression.Constant(setNullableType ?? setType), right, Expression.Constant(true)),
                                setType)));
            }
            return Expression.IfThen(
                Expression.NotEqual(Expression.Convert(getMethod, typeof(object)), StaticExpressions.NullConstant),
                Expression.Assign(left,
                    Expression.Convert(
                        Expression.Call(typeof(Convert).GetMethod("ChangeType", new Type[] { typeof(object), typeof(Type) }), Expression.Convert(right, typeof(object)), Expression.Constant(setNullableType ?? setType)),
                        setType)));
        }

        public virtual BlockExpression MapCollection(Type srcColtype, Type destColType, Expression srcExpression,
            Expression destExpression)
        {
            var sourceType = GetCollectionElementType(srcColtype);
            var destType = GetCollectionElementType(destColType);
            var sourceVariable = Expression.Variable(srcExpression.Type,
                string.Format("{0}Src", Guid.NewGuid().ToString().Replace("-", "_")));
            var assignSourceFromProp = Expression.Assign(sourceVariable, srcExpression);

            var destList = typeof(List<>).MakeGenericType(destType);
            var destColl = Expression.Variable(destList, string.Format("{0}Dst", Guid.NewGuid().ToString().Replace("-", "_")));

            var newColl = Expression.New(destList);
            var destAssign = Expression.Assign(destColl, newColl);

            var closedEnumeratorSourceType = typeof(IEnumerator<>).MakeGenericType(sourceType);
            var closedEnumerableSourceType = GenericEnumerableType.MakeGenericType(sourceType);
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
                string.Format("{0}ItmDst", Guid.NewGuid().ToString().Replace("-", "_")));

            var loopExpression = CollectionLoopExpression(destColl, sourceColItmVariable, destColItmVariable,
                assignSourceItmFromProp, doMoveNext);

            var resultCollection = ConvertCollection(destExpression.Type, destList, destType, destColl);

            var assignResult = Expression.Assign(destExpression, resultCollection);

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
                Expression.IfThenElse(Expression.Equal(srcExpression, StaticExpressions.NullConstant),
                    Expression.Assign(destExpression, Expression.Default(destExpression.Type)), blockExpression);
            var blockResultExp = Expression.Block(new ParameterExpression[] { }, new Expression[] { checkSrcForNullExp });

            return blockResultExp;
        }

        public Expression GetDifferentTypeMemberMappingExpression(Expression srcExpression, Expression destExpression)
        {
            var sourceType = srcExpression.Type;
            var destType = destExpression.Type;

            var tCol =
                sourceType.GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == GenericEnumerableType) ??
                (sourceType.IsGenericType
                    && sourceType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? sourceType
                    : null);

            var tnCol = destType.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == GenericEnumerableType) ??
                        (destType.IsGenericType && destType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? destType
                            : null);



            var blockExpression = (tCol != null && tnCol != null)
                ? MapCollection(tCol, tnCol, srcExpression, destExpression)
                : MapProperty(sourceType, destType, srcExpression, destExpression);


            var refSrcType = sourceType.IsClass;
            var destPropType = destType;
            
            if (!refSrcType) return blockExpression;

            var resultExpression =
                Expression.IfThenElse(Expression.Equal(srcExpression, StaticExpressions.NullConstant),
                    Expression.Assign(destExpression, Expression.Default(destPropType)),
                    blockExpression);
            return resultExpression;
        }

        public abstract BlockExpression MapProperty(Type srcType, Type destType, Expression srcExpression, Expression destExpression);

        protected BlockExpression CompileCollectionInternal<T, TN>(ParameterExpression sourceParameterExp, ParameterExpression destParameterExp = null)
        {
            var sourceType = GetCollectionElementType(typeof(T));
            var destType = GetCollectionElementType(typeof(TN));

            var destList = typeof(List<>).MakeGenericType(destType);
            var destColl = Expression.Variable(destList, "destColl");

            var newColl = Expression.New(destList);
            var destAssign = Expression.Assign(destColl, newColl);

            var closedEnumeratorSourceType = typeof(IEnumerator<>).MakeGenericType(sourceType);
            var closedEnumerableSourceType = GenericEnumerableType.MakeGenericType(sourceType);
            var enumerator = Expression.Variable(closedEnumeratorSourceType, "srcEnumerator");
            var assignToEnum = Expression.Assign(enumerator,
                Expression.Call(sourceParameterExp, closedEnumerableSourceType.GetMethod("GetEnumerator")));
            var doMoveNext = Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext"));

            var current = Expression.Property(enumerator, "Current");
            var sourceColItmVariable = Expression.Variable(sourceType, "ItmSrc");
            var assignSourceItmFromProp = Expression.Assign(sourceColItmVariable, current);

            var destColItmVariable = Expression.Variable(destType, "ItmDst");

            var loopExpression = CollectionLoopExpression(destColl, sourceColItmVariable, destColItmVariable,
                assignSourceItmFromProp, doMoveNext);

            var resultCollection = ConvertCollection(typeof(TN), destList, destType, destColl);

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

        internal LoopExpression CollectionLoopExpression(
            ParameterExpression destColl,
            ParameterExpression sourceColItmVariable,
            ParameterExpression destColItmVariable,
            BinaryExpression assignSourceItmFromProp,
            MethodCallExpression doMoveNext)
        {
            var mapExprForType = GetMemberMappingExpression(destColItmVariable, sourceColItmVariable);

            var addToNewColl = Expression.Call(destColl, "Add", null, destColItmVariable);

            var ifTrueBlock = Expression.Block(new[] { sourceColItmVariable, destColItmVariable }, new[] { assignSourceItmFromProp, mapExprForType, addToNewColl });

            var loopExpression = CreateLoopExpression(doMoveNext, ifTrueBlock);

            return loopExpression;
        }

        protected LoopExpression CreateLoopExpression(Expression doMoveNextSrc, BlockExpression innerLoopBlock)
        {
            var brk = Expression.Label();
            var loopExpression = Expression.Loop(
                Expression.IfThenElse(Expression.NotEqual(doMoveNextSrc, StaticExpressions.FalseConstant),
                    innerLoopBlock
                    , Expression.Break(brk))
                , brk);
            return loopExpression;
        }

        internal static Expression ConvertCollection(Type destPropType,
            Type destList,
            Type destType,
            Expression destColl)
        {
            if (destPropType.IsArray)
            {
                return Expression.Call(destColl, destList.GetMethod("ToArray"));
            }
            if (!destPropType.IsGenericType) return destColl;

            if (typeof(IQueryable).IsAssignableFrom(destPropType))
            {
                return Expression.Call(typeof(Queryable), "AsQueryable", new[] { destType }, destColl);
            }

            if (destPropType.IsInterface && destColl.Type.IsSubclassOf(destPropType))
            {
                return destColl;
            }

            if (destPropType.IsClass)
            {

            }

            return destPropType.IsClass ? Expression.New(destPropType.GetConstructor(new Type[] { destList }), destColl) : destColl;

            //var collectionType = typeof(Collection<>).MakeGenericType(destType);

            //return destPropType == collectionType ? Expression.New(collectionType.GetConstructor(new Type[] { destList }), destColl) : destColl;
        }
    }
}

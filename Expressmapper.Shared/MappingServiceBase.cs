using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressMapper
{
    internal abstract class MappingServiceBase
    {
        private readonly Type _enumImplicitConversionType = typeof(int);
        protected readonly Dictionary<long, BlockExpression> CustomTypeMapperExpCache = new Dictionary<long, BlockExpression>();
        public IDictionary<long, ITypeMapper> TypeMappers { get; set; }
        protected IMappingServiceProvider MappingServiceProvider { get; private set; }
        internal MappingServiceBase(IMappingServiceProvider mappingServiceProvider)
        {
            MappingServiceProvider = mappingServiceProvider;
            TypeMappers = new Dictionary<long, ITypeMapper>();
        }

        protected static readonly Type GenericEnumerableType = typeof(IEnumerable<>);
        public abstract IDictionary<long, MulticastDelegate> CollectionMappers { get; }
        public abstract void PrecompileCollection<T, TN>();
        public abstract bool DestinationSupport { get; }
        public abstract MulticastDelegate MapCollection(long cacheKey);
        public void Reset()
        {
            CollectionMappers.Clear();
            TypeMappers.Clear();
        }

        public void Compile(CompilationTypes compilationType)
        {
            var typeMappers = new Dictionary<long, ITypeMapper>(TypeMappers);
            foreach (var typeMapper in typeMappers)
            {
                typeMapper.Value.Compile(compilationType);
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

            throw new MapNotImplementedException(string.Format("There is no mapping has been found. Source Type: {0}, Destination Type: {1}", src.FullName, dest.FullName));
        }

        private void RegisterDynamic<T, TN>(T src, TN dest)
        {
            MappingServiceProvider.Register<T, TN>();
        }

        protected void CompileGenericCustomTypeMapper(Type srcType, Type dstType, ICustomTypeMapper typeMapper, long cacheKey)
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
            var methodInfo = customGenericType.GetInfo().GetMethod("Map");
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

        public Expression GetMemberMappingExpression(Expression left, Expression right, bool newDest)
        {
            var nullCheckNestedMemberVisitor = new NullCheckNestedMemberVisitor(false);
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

                    var releaseExp = Expression.Block(new ParameterExpression[] { }, (right.Type.GetInfo().IsPrimitive || right.Type.GetInfo().IsValueType ? resultBlockExp : (Expression)checkNullExp));

                    return releaseExp;
                }

                var returnTypeDifferenceVisitor = new ReturnTypeDifferenceVisitor(right);
                returnTypeDifferenceVisitor.Visit(right);

                // If right is custom member expression / func and return type matches left type
                // just assign
                if (left.Type == right.Type && returnTypeDifferenceVisitor.DifferentReturnTypes)
                {
                    return Expression.Assign(left, right);
                }

                if (destType.GetInfo().IsEnum && sourceType.GetInfo().IsEnum)
                {
                    return Expression.Assign(left, Expression.Convert(Expression.Convert(right, _enumImplicitConversionType), destType));
                }

                if (typeof(IConvertible).GetInfo().IsAssignableFrom(destType) &&
                    typeof(IConvertible).GetInfo().IsAssignableFrom(sourceType))
                {
                    var assignExp = CreateConvertibleAssignExpression(left,
                        right,
                        left.Type,
                        sourceType,
                        destNullableType);

                    return assignExp;
                }
                var mapComplexResult = GetDifferentTypeMemberMappingExpression(right, left, newDest);

                //                return nullCheckNestedMemberVisitor.CheckNullExpression != null
                //                    ? Expression.Condition(nullCheckNestedMemberVisitor.CheckNullExpression,
                //                        Expression.Assign(left, Expression.Default(left.Type)),
                //                        mapComplexResult)
                return mapComplexResult;
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

            return Expression.Condition(Expression.NotEqual(left, right), Expression.Assign(left, right), Expression.Default(setType));
        }

        private static Expression CreateConvertibleAssignExpression(Expression setMethod, Expression getMethod, Type setType, Type getType, Type setNullableType)
        {
            var left = setMethod;
            var right = getMethod;

            if ((setNullableType ?? setType).GetInfo().IsEnum && (getType == typeof(string)))
            {
                return Expression.IfThen(
                    Expression.NotEqual(getMethod, StaticExpressions.NullConstant),
                        Expression.Assign(left,
                            Expression.Convert(
                                Expression.Call(typeof(Enum).GetInfo().GetMethod("Parse", new Type[] { typeof(Type), typeof(string), typeof(bool) }), Expression.Constant(setNullableType ?? setType), right, Expression.Constant(true)),
                                setType)));
            }
            return Expression.IfThen(
                Expression.NotEqual(Expression.Convert(getMethod, typeof(object)), StaticExpressions.NullConstant),
                Expression.Assign(left,
                    Expression.Convert(
                        Expression.Call(typeof(Convert).GetInfo().GetMethod("ChangeType", new Type[] { typeof(object), typeof(Type) }), Expression.Convert(right, typeof(object)), Expression.Constant(setNullableType ?? setType)),
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
                Expression.Call(sourceVariable, closedEnumerableSourceType.GetInfo().GetMethod("GetEnumerator")));
            var doMoveNext = Expression.Call(enumerator, typeof(IEnumerator).GetInfo().GetMethod("MoveNext"));

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

        public Expression GetDifferentTypeMemberMappingExpression(Expression srcExpression, Expression destExpression, bool newDest)
        {
            var sourceType = srcExpression.Type;
            var destType = destExpression.Type;

            var tCol = GetEnumerableInterface(sourceType);
            var tnCol = GetEnumerableInterface(destType);

            var blockExpression = (tCol != null && tnCol != null)
                ? MapCollection(tCol, tnCol, srcExpression, destExpression)
                : MapProperty(sourceType, destType, srcExpression, destExpression, newDest);

            var refSrcType = sourceType.GetInfo().IsClass;
            var destPropType = destType;

            if (!refSrcType) return blockExpression;

            var resultExpression =
                Expression.IfThenElse(Expression.Equal(srcExpression, StaticExpressions.NullConstant),
                    Expression.Assign(destExpression, Expression.Default(destPropType)),
                    blockExpression);
            return resultExpression;
        }

        public abstract BlockExpression MapProperty(Type srcType, Type destType, Expression srcExpression, Expression destExpression, bool newDest);

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
                Expression.Call(sourceParameterExp, closedEnumerableSourceType.GetInfo().GetMethod("GetEnumerator")));
            var doMoveNext = Expression.Call(enumerator, typeof(IEnumerator).GetInfo().GetMethod("MoveNext"));

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
            return type.IsArray ? type.GetElementType() : GetEnumerableInterface(type).GetInfo().GetGenericArguments()[0];
        }

        internal LoopExpression CollectionLoopExpression(
            ParameterExpression destColl,
            ParameterExpression sourceColItmVariable,
            ParameterExpression destColItmVariable,
            BinaryExpression assignSourceItmFromProp,
            MethodCallExpression doMoveNext)
        {
            var mapExprForType = GetMemberMappingExpression(destColItmVariable, sourceColItmVariable, true);

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
                return Expression.Call(destColl, destList.GetInfo().GetMethod("ToArray"));
            }

            if (!destPropType.GetInfo().IsGenericType && GetEnumerableInterface(destPropType) == null)
            {
                return destColl;
            }

            if (typeof(IQueryable).GetInfo().IsAssignableFrom(destPropType))
            {
                return Expression.Call(typeof(Queryable), "AsQueryable", new[] { destType }, destColl);
            }

            if (destPropType.GetInfo().IsInterface && destPropType.GetInfo().IsAssignableFrom(destColl.Type))
            {
                // This will handle any destination interface implemented by List<T>
                return destColl;
            }

            ConstructorInfo ctor = null;

            if (destPropType.GetInfo().IsInterface)
            {
                // We are targeting an interface type, we need to find a compatible collection type
                // We could look for a loaded type that implements the target interface and has an appropriate
                // constructor, but that is a bit too much magic for now.
                throw new NotImplementedException(
                    $"Destination interface type {destPropType.FullName} is not supported yet");
            }
            else
            {
                ctor = destPropType.GetInfo().GetConstructors(BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(
                    ci => {
                        var param = ci.GetParameters();
                        return param.Length == 1 && param[0].ParameterType.GetInfo().IsAssignableFrom(destList);
                    });

                if (ctor == null)
                {
                    throw new Exception($"Could not find a constructor on {destPropType.Name} that accepts {destList}");
                }
            }

            return Expression.New(ctor, destColl);
        }

        public static Type GetEnumerableInterface(Type type)
        {
            return
                type.GetInfo().GetInterfaces()
                    .FirstOrDefault(t => t.GetInfo().IsGenericType && t.GetGenericTypeDefinition() == GenericEnumerableType)
                ?? (type.GetInfo().IsGenericType && type.GetInfo().GetInterfaces().Any(t => t == typeof(IEnumerable)) ? type : null);
        }
    }
}

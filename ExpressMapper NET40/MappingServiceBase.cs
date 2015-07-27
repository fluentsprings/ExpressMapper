using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressMapper
{
    internal abstract class MappingServiceBase : IMappingService
    {
        protected IMappingServiceProvider MappingServiceProvider { get; private set; }
        internal MappingServiceBase(IMappingServiceProvider mappingServiceProvider)
        {
            MappingServiceProvider = mappingServiceProvider;
        }

        protected static readonly Type GenericEnumerableType = typeof(IEnumerable<>);
        public abstract IDictionary<int, MulticastDelegate> CollectionMappers { get; }
        public abstract void PrecompileCollection<T, TN>();
        public abstract bool DestinationSupport { get; }
        public abstract MulticastDelegate MapCollection(int cacheKey);
        public void Reset()
        {
            CollectionMappers.Clear();
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
            var destColl = Expression.Variable(destList, string.Format("{0}Dest", Guid.NewGuid().ToString().Replace("-", "_")));

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
                string.Format("{0}ItmDest", Guid.NewGuid().ToString().Replace("-", "_")));

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
            if (refSrcType)
            {
                var resultExpression =
                    Expression.IfThenElse(Expression.Equal(srcExpression, StaticExpressions.NullConstant),
                        Expression.Assign(destExpression, Expression.Default(destPropType)),
                        blockExpression);
                return resultExpression;
            }
            return blockExpression;
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

            var destColItmVariable = Expression.Variable(destType, "ItmDest");

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
            var mapExprForType = MappingServiceProvider.GetMemberMappingExpression(destColItmVariable, sourceColItmVariable).Item1;

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
    }
}

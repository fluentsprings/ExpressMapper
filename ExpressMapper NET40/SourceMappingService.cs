using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressMapper
{
    internal class SourceMappingService : MappingServiceBase, IMappingService
    {
        private const string Mscorlib_Str = "mscorlib";
        private const string System_Namespace_Str = "System";

        #region Constructors

        public SourceMappingService(IMappingServiceProvider mappingServiceProvider)
            : base(mappingServiceProvider)
        {
        }

        #endregion

        #region Privates

        private readonly IDictionary<long, MulticastDelegate> _collectionMappers =
            new Dictionary<long, MulticastDelegate>();

        #endregion

        #region Implementation of IMappingService

        public override IDictionary<long, MulticastDelegate> CollectionMappers
        {
            get { return _collectionMappers; }
        }

        public override void PrecompileCollection<T, TN>()
        {
            var cacheKey = MappingServiceProvider.CalculateCacheKey(typeof(T), typeof(TN));
            if (CollectionMappers.ContainsKey(cacheKey)) return;

            var sourceParameterExp = Expression.Parameter(typeof (T), "sourceColl");
            var blockExp = CompileCollectionInternal<T, TN>(sourceParameterExp);
            var lambda = Expression.Lambda<Func<T, TN>>(blockExp, sourceParameterExp);
            var compiledFunc = lambda.Compile();
            CollectionMappers[cacheKey] = compiledFunc;
        }

        public override bool DestinationSupport
        {
            get { return false; }
        }

        public override MulticastDelegate MapCollection(long cacheKey)
        {
            return CollectionMappers.ContainsKey(cacheKey) ? CollectionMappers[cacheKey] : null;
        }

        protected override bool ComplexMapCondition(Type src, Type dst)
        {
            var tCol =
                src.GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == GenericEnumerableType) ??
                (src.IsGenericType
                    && src.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? src
                    : null);

            var tnCol = dst.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == GenericEnumerableType) ??
                        (dst.IsGenericType && dst.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? dst
                            : null);
            if (tCol == null || tnCol == null || (src == typeof(string) && dst == typeof(string)))
                return (base.ComplexMapCondition(src, dst) ||
                        (src == dst && src.IsClass && !src.Assembly.FullName.Contains(Mscorlib_Str) &&
                         !src.FullName.StartsWith(System_Namespace_Str)));
            return true;
        }

        public override BlockExpression MapProperty(Type srcType, Type destType, Expression srcExpression, Expression destExpression, bool newDest)
        {
            var sourceVariable = Expression.Variable(srcType,
                string.Format("{0}_{1}Src", srcType.Name, Guid.NewGuid().ToString().Replace("-", "_")));
            var assignSourceFromProp = Expression.Assign(sourceVariable, srcExpression);
            var exprForType = GetMapExpressions(srcType, destType);
            var mapExprForType = exprForType.Item1;
            var destVariable = Expression.Variable(destType,
                string.Format("{0}_{1}Dst", destType.Name,
                    Guid.NewGuid().ToString().Replace("-", "_")));
            var blockForSubstitution = Expression.Block(mapExprForType);

            var substBlock =
                new PreciseSubstituteParameterVisitor(
                    new KeyValuePair<ParameterExpression, ParameterExpression>(exprForType.Item2, sourceVariable),
                    new KeyValuePair<ParameterExpression, ParameterExpression>(exprForType.Item3, destVariable))
                    .Visit(blockForSubstitution) as
                    BlockExpression;
            //var substBlock =
            //    new SubstituteParameterVisitor(sourceVariable, destVariable).Visit(blockForSubstitution) as
            //        BlockExpression;
            var resultMapExprForType = substBlock.Expressions;

            var assignExp = Expression.Assign(destExpression, destVariable);

            var expressions = new List<Expression> { assignSourceFromProp };
            expressions.AddRange(resultMapExprForType);
            expressions.Add(assignExp);

            var parameterExpressions = new List<ParameterExpression> { sourceVariable, destVariable };
            var blockExpression = Expression.Block(parameterExpressions, expressions);

            return blockExpression;
        }

        public Expression GetMemberQueryableExpression(Type srcType, Type dstType)
        {
            var cacheKey = MappingServiceProvider.CalculateCacheKey(srcType, dstType);
            if (!TypeMappers.ContainsKey(cacheKey)) return null;

            var typeMapper = TypeMappers[cacheKey];
            if (typeMapper.QueryableGeneralExpression == null)
            {
                typeMapper.Compile();
            }
            return typeMapper.QueryableGeneralExpression;
        }

        #endregion
    }
}

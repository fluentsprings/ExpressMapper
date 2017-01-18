using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressMapper
{
    internal class SourceMappingService : MappingServiceBase, IMappingService
    {
        private const string MscorlibStr = "mscorlib";
        private const string SystemNamespaceStr = "System";

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

        public override IDictionary<long, MulticastDelegate> CollectionMappers => _collectionMappers;

        public override void PrecompileCollection<T, TN>()
        {
            var cacheKey = MappingServiceProvider.CalculateCacheKey(typeof(T), typeof(TN));
            if (CollectionMappers.ContainsKey(cacheKey)) return;

            var sourceParameterExp = Expression.Parameter(typeof(T), "sourceColl");
            var blockExp = CompileCollectionInternal<T, TN>(sourceParameterExp);
            var lambda = Expression.Lambda<Func<T, TN>>(blockExp, sourceParameterExp);
            var compiledFunc = lambda.Compile();
            CollectionMappers[cacheKey] = compiledFunc;
        }

        public override bool DestinationSupport => false;

        public override MulticastDelegate MapCollection(long cacheKey)
        {
            return CollectionMappers.ContainsKey(cacheKey) ? CollectionMappers[cacheKey] : null;
        }

        protected override bool ComplexMapCondition(Type src, Type dst)
        {
            var tCol =
                src.GetInfo().GetInterfaces()
                    .FirstOrDefault(t => t.GetInfo().IsGenericType && t.GetGenericTypeDefinition() == GenericEnumerableType) ??
                (src.GetInfo().IsGenericType
                 && src.GetInfo().GetInterfaces().Any(t => t == typeof(IEnumerable)) ? src
                    : null);

            var tnCol = dst.GetInfo().GetInterfaces()
                            .FirstOrDefault(t => t.GetInfo().IsGenericType && t.GetGenericTypeDefinition() == GenericEnumerableType) ??
                        (dst.GetInfo().IsGenericType && dst.GetInfo().GetInterfaces().Any(t => t == typeof(IEnumerable)) ? dst
                            : null);
            if (tCol == null || tnCol == null || (src == typeof(string) && dst == typeof(string)))
                return (base.ComplexMapCondition(src, dst) ||
                        (src == dst && src.GetInfo().IsClass && !src.GetInfo().Assembly.FullName.Contains(MscorlibStr) &&
                         !src.FullName.StartsWith(SystemNamespaceStr)));
            return true;
        }

        public override BlockExpression MapProperty(Type srcType, Type destType, Expression srcExpression, Expression destExpression, bool newDest)
        {
            var sourceVariable = Expression.Variable(srcType,
                $"{srcType.Name}_{Guid.NewGuid().ToString().Replace("-", "_")}Src");
            var assignSourceFromProp = Expression.Assign(sourceVariable, srcExpression);
            var exprForType = GetMapExpressions(srcType, destType);
            var mapExprForType = exprForType.Item1;
            var destVariable = Expression.Variable(destType,
                $"{destType.Name}_{Guid.NewGuid().ToString().Replace("-", "_")}Dst");
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
                typeMapper.Compile(CompilationTypes.Source);
            }
            return typeMapper.QueryableGeneralExpression;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressMapper
{
    internal class SourceMappingService : MappingServiceBase, IMappingService
    {
        #region Constructors

        public SourceMappingService(IMappingServiceProvider mappingServiceProvider)
            : base(mappingServiceProvider)
        {
        }

        #endregion

        #region Privates

        private readonly IDictionary<int, MulticastDelegate> _collectionMappers =
            new Dictionary<int, MulticastDelegate>();

        #endregion

        #region Implementation of IMappingService

        public override IDictionary<int, MulticastDelegate> CollectionMappers
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
            CollectionMappers.Add(cacheKey, compiledFunc);
        }

        public override bool DestinationSupport
        {
            get { return false; }
        }

        public override MulticastDelegate MapCollection(int cacheKey)
        {
            return CollectionMappers.ContainsKey(cacheKey) ? CollectionMappers[cacheKey] : null;
        }

        public override BlockExpression MapProperty(Type srcType, Type destType, Expression srcExpression, Expression destExpression)
        {
            var sourceVariable = Expression.Variable(srcType,
                string.Format("{0}_{1}Src", srcType.Name, Guid.NewGuid().ToString().Replace("-", "_")));
            var assignSourceFromProp = Expression.Assign(sourceVariable, srcExpression);
            var mapExprForType = GetMapExpressions(srcType, destType);
            var destVariable = Expression.Variable(destType,
                string.Format("{0}_{1}Dest", destType.Name,
                    Guid.NewGuid().ToString().Replace("-", "_")));
            var blockForSubstitution = Expression.Block(mapExprForType);
            var substBlock =
                new SubstituteParameterVisitor(sourceVariable, destVariable).Visit(blockForSubstitution) as
                    BlockExpression;
            var resultMapExprForType = substBlock.Expressions;

            var assignExp = Expression.Assign(destExpression, destVariable);

            var expressions = new List<Expression> { assignSourceFromProp };
            expressions.AddRange(resultMapExprForType);
            expressions.Add(assignExp);

            var parameterExpressions = new List<ParameterExpression> { sourceVariable, destVariable };
            var blockExpression = Expression.Block(parameterExpressions, expressions);

            return blockExpression;
        }

        #endregion
    }
}

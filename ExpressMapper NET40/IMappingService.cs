using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressMapper
{
    internal interface IMappingService
    {
        IDictionary<int, MulticastDelegate> CollectionMappers { get; }
        void PrecompileCollection<T, TN>();
        bool DestinationSupport { get; }
        MulticastDelegate MapCollection(int cacheKey);
        void Reset();
        BlockExpression MapCollection(Type srcColtype, Type destColType, Expression srcExpression, Expression destExpression);
        Expression GetDifferentTypeMemberMappingExpression(Expression srcExpression, Expression destExpression);
        BlockExpression MapProperty(Type srcType, Type destType, Expression srcExpression, Expression destExpression);
    }
}

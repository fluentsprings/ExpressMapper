using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressMapper
{
    internal interface IMappingServiceProvider
    {
        void Compile();
        void PrecompileCollection<T, TN>();
        TN Map<T, TN>(T src, TN dest = default(TN));
        object Map(Type srcType, Type dstType, object src, object dest = null);
        IMemberConfiguration<T, TN> Register<T, TN>();
        void RegisterCustom<T, TN, TMapper>() where TMapper : ICustomTypeMapper<T, TN>;
        void RegisterCustom<T, TN>(Func<T, TN> mapFunc);
        void Reset();
        Tuple<Expression, Expression> GetMemberMappingExpression(Expression left, Expression right);
        int CalculateCacheKey(Type src, Type dest);
        IList<Expression> GetMapExpressions(Type src, Type dest, bool withDestinationInstance);
    }
}

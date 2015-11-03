using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressMapper
{
    public interface ITypeMapper
    {
        Expression QueryableGeneralExpression { get; }
        Func<object, object, object> GetNonGenericMapFunc();
        Tuple<List<Expression>, ParameterExpression, ParameterExpression> GetMapExpressions();
        void Compile();
    }

    /// <summary>
    /// Interface that implements internals of mapping
    /// </summary>
    /// <typeparam name="T">source</typeparam>
    /// <typeparam name="TN">destination</typeparam>
    public interface ITypeMapper<T, TN> : ITypeMapper
    {
        Expression<Func<T, TN>> QueryableExpression { get; }
        TN MapTo(T src, TN dest);
        void Ignore<TMember>(Expression<Func<TN, TMember>> left);
        void MapMember<TMember, TNMember>(Expression<Func<TN, TNMember>> left, Expression<Func<T, TMember>> right);
        void MapFunction<TMember, TNMember>(Expression<Func<TN, TNMember>> left, Func<T, TMember> right);
        void Instantiate(Func<T,TN> constructor);
        void BeforeMap(Action<T,TN> beforeMap);
        void AfterMap(Action<T,TN> afterMap);
    }
}

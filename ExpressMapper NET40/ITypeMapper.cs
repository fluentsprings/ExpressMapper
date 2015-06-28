using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressMapper
{
    public interface ITypeMapper
    {
        Func<object, object> GetNonGenericMapFunc();
        List<Expression> GetMapExpressions(bool withDestinationInstance = false);
        IList ProcessCollection(IEnumerable src);
        IEnumerable ProcessArray(IEnumerable src);
        IQueryable ProcessQueryable(IEnumerable src);
        IList ProcessCollection(IEnumerable src, IList dest);
        IEnumerable ProcessArray(IEnumerable src, IEnumerable dest);
        IQueryable ProcessQueryable(IEnumerable src, IQueryable dest);
        void Compile();
        void CompileDestinationInstance();
    }

    /// <summary>
    /// Interface that implements internals of mapping
    /// </summary>
    /// <typeparam name="T">source</typeparam>
    /// <typeparam name="TN">destination</typeparam>
    public interface ITypeMapper<T, TN> : ITypeMapper
    {
        TN MapTo(T src);
        TN MapTo(T src, TN dest);
        void Custom(ICustomTypeMapper<T, TN> customTypeMapper);
        void Ignore<TMember>(Expression<Func<TN, TMember>> left);
        void MapMember<TMember, TNMember>(Expression<Func<TN, TNMember>> left, Expression<Func<T, TMember>> right);
        void MapFunction<TMember, TNMember>(Expression<Func<TN, TNMember>> left, Func<T, TMember> right);
        void Instantiate(Func<T,TN> constructor);
        void BeforeMap(Action<T,TN> beforeMap);
        void AfterMap(Action<T,TN> afterMap);
    }
}

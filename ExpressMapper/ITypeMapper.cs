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
        List<Expression> GetMapExpressions();
        IList ProcessCollection(IEnumerable src);
        IEnumerable ProcessArray(IEnumerable src);
        IQueryable ProcessQueryable(IEnumerable src);
        void Compile();
    }

    public interface ITypeMapper<T, TN> : ITypeMapper
    {
        TN MapTo(T obj);
        void Custom(ICustomTypeMapper<T, TN> customTypeMapper);
        void Ignore<TMember>(Expression<Func<TN, TMember>> left);
        void MapMember<TMember, TNMember>(Expression<Func<TN, TNMember>> left, Expression<Func<T, TMember>> right);
        void MapFunction<TMember, TNMember>(Expression<Func<TN, TNMember>> left, Func<T, TMember> right);
        void Instantiate(Func<T,TN> constructor);
        void BeforeMap(Action<T,TN> beforeMap);
        void AfterMap(Action<T,TN> afterMap);
        void AutoMapProperty(PropertyInfo propertyGet, PropertyInfo propertySet);
    }
}

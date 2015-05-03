using System;
using System.Linq.Expressions;

namespace ExpressMapper
{
    public interface IMemberConfiguration<T, TN>
    {
        IMemberConfiguration<T, TN> Instantiate(Func<T, TN> constructor);
        IMemberConfiguration<T, TN> Before(Action<T, TN> beforeHandler);
        IMemberConfiguration<T, TN> After(Action<T, TN> afterHandler);
        IMemberConfiguration<T, TN> Member<TMember, TNMember>(Expression<Func<TN, TNMember>> dest, Expression<Func<T, TMember>> src);
        IMemberConfiguration<T, TN> Function<TMember, TNMember>(Expression<Func<TN, TNMember>> dest, Func<T, TMember> src);
        IMemberConfiguration<T, TN> Ignore<TMember>(Expression<Func<TN, TMember>> dest);
        void Custom(ICustomTypeMapper<T, TN> customTypeMapper);
    }
}

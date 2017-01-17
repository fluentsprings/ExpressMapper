using System;
using System.Linq.Expressions;

namespace ExpressMapper
{
    /// <summary>
    /// Interface to extend the mapping
    /// </summary>
    /// <typeparam name="T">source</typeparam>
    /// <typeparam name="TN">destination</typeparam>
    public interface IMemberConfiguration<T, TN>
    {
        IMemberConfiguration<T, TN> InstantiateFunc(Func<T, TN> constructor);
        IMemberConfiguration<T, TN> Instantiate(Expression<Func<T, TN>> constructor);
        IMemberConfiguration<T, TN> Before(Action<T, TN> beforeHandler);
        IMemberConfiguration<T, TN> After(Action<T, TN> afterHandler);
        IMemberConfiguration<T, TN> Member<TMember, TNMember>(Expression<Func<TN, TNMember>> dest, Expression<Func<T, TMember>> src);
        IMemberConfiguration<T, TN> Function<TMember, TNMember>(Expression<Func<TN, TNMember>> dest, Func<T, TMember> src);
        IMemberConfiguration<T, TN> Ignore<TMember>(Expression<Func<TN, TMember>> dest);
        IMemberConfiguration<T, TN> Value<TNMember>(Expression<Func<TN, TNMember>> dest, TNMember value);
        IMemberConfiguration<T, TN> CaseSensitive(bool caseSensitive);
        IMemberConfiguration<T, TN> CompileTo(CompilationTypes compilationType);
        IMemberConfiguration<T, TN> Flatten();

        IMemberConfiguration<T, TN> Include<TSub, TNSub>() where TSub : T
            where TNSub : TN;
    }
}

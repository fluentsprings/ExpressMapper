using System;
using System.Linq.Expressions;

namespace ExpressMapper
{
    public class MemberConfiguration<T, TN> : IMemberConfiguration<T, TN>
    {
        private readonly ITypeMapper<T, TN> _typeMapper; 
        public MemberConfiguration(ITypeMapper<T, TN> typeMapper)
        {
            _typeMapper = typeMapper;
        } 

        public IMemberConfiguration<T, TN> Instantiate(Func<T, TN> constructor)
        {
            _typeMapper.Instantiate(constructor);
            return this;
        }

        public IMemberConfiguration<T, TN> Before(Action<T, TN> beforeHandler)
        {
            _typeMapper.BeforeMap(beforeHandler);
            return this;
        }

        public IMemberConfiguration<T, TN> After(Action<T, TN> afterHandler)
        {
            _typeMapper.AfterMap(afterHandler);
            return this;
        }

        public IMemberConfiguration<T, TN> Base<TBase>(Expression<Action<T, TN>> afterHandler) where TBase : class, new()
        {
            throw new NotImplementedException();
        }

        public IMemberConfiguration<T, TN> Member<TMember, TNMember>(Expression<Func<TN, TNMember>> dest, Expression<Func<T, TMember>> src)
        {
            var memberExpression = dest.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new Exception("MemberExpression should return one of the properties of destination class");
            }

            var propertyInfo = typeof(TN).GetProperty(memberExpression.Member.Name);

            if (propertyInfo.CanWrite && propertyInfo.GetSetMethod(true).IsPublic)
            {
                _typeMapper.MapMember(dest, src);
            }
            return this;
        }

        public IMemberConfiguration<T, TN> Function<TMember, TNMember>(Expression<Func<TN, TNMember>> dest, Func<T, TMember> src)
        {
            var memberExpression = dest.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new Exception("MemberExpression should return one of the properties of destination class");
            }

            var propertyInfo = typeof(TN).GetProperty(memberExpression.Member.Name);

            if (propertyInfo.CanWrite && propertyInfo.GetSetMethod(true).IsPublic)
            {
                _typeMapper.MapFunction(dest, src);
            }
            return this;
        }

        public IMemberConfiguration<T, TN> Ignore<TMember>(Expression<Func<TN, TMember>> dest)
        {
            if (!(dest.Body is MemberExpression))
            {
                throw new Exception("MemberExpression should return one of the properties of destination class");
            }
            _typeMapper.Ignore(dest);
            return this;
        }

        //public void Custom(ICustomTypeMapper<T, TN> customTypeMapper)
        //{
        //    _typeMapper.Custom(customTypeMapper);
        //}
    }
}

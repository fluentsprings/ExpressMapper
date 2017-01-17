using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressMapper
{
    public class MemberConfiguration<T, TN> : IMemberConfiguration<T, TN>
    {
        private readonly IMappingServiceProvider _mappingServiceProvider;
        private readonly ITypeMapper<T, TN>[] _typeMappers;
        public MemberConfiguration(ITypeMapper<T, TN>[] typeMappers, IMappingServiceProvider mappingServiceProvider)
        {
            _typeMappers = typeMappers;
            _mappingServiceProvider = mappingServiceProvider;
        }

        public IMemberConfiguration<T, TN> InstantiateFunc(Func<T, TN> constructor)
        {
            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.InstantiateFunc(constructor);
            }
            return this;
        }

        public IMemberConfiguration<T, TN> Instantiate(Expression<Func<T, TN>> constructor)
        {
            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.Instantiate(constructor);
            }
            return this;
        }

        public IMemberConfiguration<T, TN> Before(Action<T, TN> beforeHandler)
        {
            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.BeforeMap(beforeHandler);
            }
            return this;
        }

        public IMemberConfiguration<T, TN> After(Action<T, TN> afterHandler)
        {
            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.AfterMap(afterHandler);
            }
            return this;
        }

        public IMemberConfiguration<T, TN> Base<TBase>(Expression<Action<T, TN>> afterHandler) where TBase : class, new()
        {
            throw new NotImplementedException();
        }

        public IMemberConfiguration<T, TN> Member<TMember, TNMember>(Expression<Func<TN, TNMember>> dest, Expression<Func<T, TMember>> src)
        {
            if (dest == null)
            {
                throw new ArgumentNullException("dst");
            }

            var memberExpression = dest.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new Exception("MemberExpression should return one of the properties of destination class");
            }

            var propertyInfo = memberExpression.Member as PropertyInfo;

            if (propertyInfo != null && !propertyInfo.CanWrite || (propertyInfo != null && propertyInfo.CanWrite && !propertyInfo.GetSetMethod(true).IsPublic))
            {
                Ignore(dest);
            }
            else
            {
                foreach (var typeMapper in _typeMappers)
                {
                    typeMapper.MapMember(dest, src);
                }
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

            var propertyInfo = memberExpression.Member as PropertyInfo;

            if (propertyInfo != null && !propertyInfo.CanWrite || (propertyInfo != null && propertyInfo.CanWrite && !propertyInfo.GetSetMethod(true).IsPublic))
            {
                Ignore(dest);
            }
            else
            {
                foreach (var typeMapper in _typeMappers)
                {
                    typeMapper.MapFunction(dest, src);
                }
            }
            return this;
        }

        public IMemberConfiguration<T, TN> Ignore<TMember>(Expression<Func<TN, TMember>> dest)
        {
            if (dest == null)
            {
                throw new ArgumentNullException("dst");
            }

            if (!(dest.Body is MemberExpression))
            {
                throw new Exception("MemberExpression should return one of the properties of destination class");
            }
            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.Ignore(dest);
            }
            return this;
        }

        public IMemberConfiguration<T, TN> Value<TNMember>(Expression<Func<TN, TNMember>> dest, TNMember value)
        {
            if (!(dest.Body is MemberExpression))
            {
                throw new Exception("MemberExpression should return one of the properties of destination class");
            }
            return Member(dest, x => value);
        }

        public IMemberConfiguration<T, TN> CaseSensitive(bool caseSensitive)
        {
            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.CaseSensetiveMemberMap(caseSensitive);
            }
            return this;
        }

        public IMemberConfiguration<T, TN> CompileTo(CompilationTypes compilationType)
        {
            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.CompileTo(compilationType);
            }
            return this;
        }

        public IMemberConfiguration<T, TN> Include<TSub, TNSub>() where TSub : T
            where TNSub : TN
        {
            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.BaseType = true;
            }

            _mappingServiceProvider.Register<TSub, TNSub>(_typeMappers.First() as IMemberConfigParameters);
            return this;
        }

        #region flatten code

        /// <summary>
        /// This will apply the flattening algorithm to the source.
        /// This allow properties in nested source classes to be assigned to a top level destination property.
        /// Matching is done by concatenated names, and also a few Linq commands
        /// e.g. dest.NestedClassMyProperty would contain the property src.NestedClass.MyProperty (as long as types match)
        /// and  dest.MyCollectionCount would contain the count (int) of the Collection.
        /// </summary>
        public IMemberConfiguration<T, TN> Flatten()
        {
            var sourceMapperBase =
                _typeMappers.Single(x => x.MapperType == CompilationTypes.Source) as TypeMapperBase<T, TN>;

            if (sourceMapperBase == null)
                throw new Exception("Failed to find the source mapping.");

            foreach (var typeMapper in _typeMappers)
            {
                typeMapper.Flatten();
            }

            return this;
        }

        #endregion

    }
}

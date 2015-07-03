using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressMapper.Ioc
{
    public class DependencyResolver : IContainer
    {
        #region Constructors

        private DependencyResolver()
        {
            Initialize();
        }

        #endregion

        #region Privates

        private readonly Dictionary<int, Dictionary<int, Delegate>> _cache = new Dictionary<int, Dictionary<int, Delegate>>();
        private static IContainer _container;

        #endregion

        internal static IContainer GetContainer()
        {
            return _container ?? (_container = new DependencyResolver());
        }

        public T Resolve<T>()
        {
            var interfHash = typeof(T).GetHashCode();
            NotImplementedCheck(interfHash, typeof(T).FullName);
            MoreThanOnImplCheck(interfHash, typeof(T).FullName);

            var implInstance = (T)_cache[interfHash].First().Value.DynamicInvoke();
            return implInstance;
        }

        #region Checks and throw exceptions

        private void MoreThanOnImplCheck(int interfHash, string typeFullName)
        {
            if (_cache[interfHash].Count > 1)
            {
                throw new NotImplementedException(
                    string.Format("There more than one implementation have been found for the {0} interface.",
                        typeFullName));
            }
        }

        private void NotImplementedCheck(int interfHash, string typeFullName)
        {
            if (!_cache.ContainsKey(interfHash) || _cache[interfHash].Count == 0)
            {
                throw new NotImplementedException(
                    string.Format("No implementation has been found for the {0} interface.", typeFullName));
            }

        }

        #endregion

        public IEnumerable<T> ResolveAll<T>()
        {
            var interfHash = typeof(T).GetHashCode();
            NotImplementedCheck(interfHash, typeof(T).FullName);
            var implArray = new T[_cache[interfHash].Count];
            for (var i = 0; i < _cache[interfHash].Values.Count; i++)
            {
                implArray[i] = (T) _cache[interfHash].Values.ElementAt(i).DynamicInvoke();
            }
            return implArray;
        }

        #region Initialization and compilation
        private void Initialize()
        {
            var expressMapperAssembly = Assembly.GetExecutingAssembly();
            var expressInterfaces = expressMapperAssembly.GetTypes().Where(t => t.IsInterface);
            foreach (var expressInterface in expressInterfaces)
            {
                var implementations = expressMapperAssembly.GetTypes().Where(t => expressInterface.IsAssignableFrom(t));
                Compile(expressInterface, implementations);
            }

        }

        private void Compile(Type inter, IEnumerable<Type> implementations)
        {
            foreach (var impl in implementations)
            {
                var interHash = inter.GetHashCode();
                if (!_cache.ContainsKey(interHash))
                {
                    _cache[interHash] = new Dictionary<int, Delegate>();
                }

                var implHash = impl.GetHashCode();

                if (!_cache[interHash].ContainsKey(implHash))
                {
                    var newExpression = Expression.New(impl);
                    var lambdaExpression = Expression.Lambda(newExpression);
                    var del = lambdaExpression.Compile();
                    _cache[interHash][implHash] = del;
                }
            }
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressMapper
{
    internal class FlattenLinqMethod
    {
        //Any name starting with ~ means the return type should not be checked because it is the same as source
        private static readonly string[] ListOfSupportedLinqMethods = new[]
        {
            "Any", "Count", "LongCount",
            "~FirstOrDefault"
            //"~First", "~Last", "~LastOrDefault", "~Single", "~SingleOrDefault"    - not supported by Entity Framework
        };

        private static readonly List<FlattenLinqMethod> EnumerableMethodLookup;

        static FlattenLinqMethod()
        {
            EnumerableMethodLookup = 
                (from givenName in ListOfSupportedLinqMethods
                let checkReturnType = givenName[0] != '~'
                let name = checkReturnType ? givenName : givenName.Substring(1)
                select new FlattenLinqMethod(name, checkReturnType) ).ToList();
        }

        /// <summary>
        /// Method name 
        /// </summary>
        private readonly string _methodName;

        /// <summary>
        /// If true then the return type should be checked to get the right version of the method
        /// </summary>
        private readonly bool _checkReturnType ;

        private FlattenLinqMethod(string methodName, bool checkReturnType)
        {
            _methodName = methodName;
            _checkReturnType = checkReturnType;
        }

        /// <summary>
        /// This can be called on enumerable properly to see if the ending is a valid Linq method
        /// </summary>
        /// <param name="endOfName"></param>
        /// <param name="stringComparison"></param>
        /// <returns></returns>
        public static FlattenLinqMethod EnumerableEndMatchsWithLinqMethod(string endOfName, StringComparison stringComparison)
        {
            return EnumerableMethodLookup.SingleOrDefault(x => string.Equals(x._methodName, endOfName, stringComparison));
        }

        public override string ToString()
        {
            return $".{_methodName}()";
        }

        public MethodCallExpression AsMethodCallExpression(Expression propertyExpression, PropertyInfo propertyToActOn, PropertyInfo destProperty)
        {
            var ienumerableType = propertyToActOn.PropertyType.GetInfo().GetGenericArguments().Single();

            var foundMethodInfo = typeof (Enumerable).GetInfo().GetMethods()
                .SingleOrDefault(m => m.Name == _methodName && m.GetParameters().Length == 1
                        && (!_checkReturnType || m.ReturnType == destProperty.PropertyType));

            if (foundMethodInfo == null)
                throw new ExpressmapperException(
                    $"We could not find the Method {_methodName}() which matched the property {destProperty.Name} of type {destProperty.PropertyType}.");

            var method = foundMethodInfo.IsGenericMethod
                ? foundMethodInfo.MakeGenericMethod(ienumerableType)
                : foundMethodInfo;

            return Expression.Call(method, propertyExpression);
        }

    }
}
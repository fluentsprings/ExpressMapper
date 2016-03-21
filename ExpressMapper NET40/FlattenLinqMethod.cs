using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

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

        private static readonly Dictionary<string, FlattenLinqMethod> EnumerableMethodLookup;

        static FlattenLinqMethod()
        {
            EnumerableMethodLookup = 
                (from givenName in ListOfSupportedLinqMethods
                let checkReturnType = givenName[0] != '~'
                let name = checkReturnType ? givenName : givenName.Substring(1)
                select new { Key = name, Value = new FlattenLinqMethod(name, checkReturnType) })
                .ToDictionary(key => key.Key, val => val.Value);
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
        /// <returns></returns>
        public static FlattenLinqMethod EnumerableEndMatchsWithLinqMethod(string endOfName)
        {
            return EnumerableMethodLookup.ContainsKey(endOfName) ? EnumerableMethodLookup[endOfName] : null;
        }

        public override string ToString()
        {
            return $".{_methodName}()";
        }

        public MethodCallExpression AsMethodCallExpression(Expression propertyExpression, PropertyInfo propertyToActOn, PropertyInfo destProperty)
        {
            var ienumerableType = propertyToActOn.PropertyType.GetGenericArguments().Single();

            var foundMethodInfo = typeof (Enumerable).GetMethods()
                .Single(m => m.Name == _methodName && m.GetParameters().Length == 1
                        && (!_checkReturnType || m.ReturnType == destProperty.PropertyType));

            if (foundMethodInfo == null)
                throw new InvalidOperationException(
                    $"We could not find the Method {_methodName} which matched the {destProperty.Name} which has the type {destProperty.PropertyType}.");

            var method = foundMethodInfo.IsGenericMethod
                ? foundMethodInfo.MakeGenericMethod(ienumerableType)
                : foundMethodInfo;

            return Expression.Call(method, propertyExpression);
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressMapper
{
    public class TypeMapper<T, TN> : ITypeMapper<T, TN>
    {
        #region Privates

        private readonly List<Expression> _giveAway = new List<Expression>();
        private readonly List<Expression> _giveWithDestinationAway = new List<Expression>();

        private readonly ParameterExpression _sourceParameter = Expression.Parameter(typeof(T), "src");
        private readonly ParameterExpression _destFakeParameter = Expression.Parameter(typeof(TN), "dest");
        private Func<T, TN> _mapFunc;
        private Func<T, TN, TN> _mapDestInstFunc;
        private readonly List<string> _ignoreList = new List<string>();
        private readonly Dictionary<string, Expression> _propertyCache = new Dictionary<string, Expression>();
        private readonly Dictionary<string, Expression> _customPropertyCache = new Dictionary<string, Expression>();

        private readonly Dictionary<string, Expression> _propertyDestInstCache = new Dictionary<string, Expression>();
        private readonly Dictionary<string, Expression> _customPropertyDestInstCache = new Dictionary<string, Expression>();

        private Action<T, TN> _beforeMapHandler;
        private Action<T, TN> _afterMapHandler;
        private Func<T, TN> _constructorFunc;

        private Func<object, object> _nonGenericMapFunc;

        private readonly MappingService _mappingService;

        #endregion

        #region Constructors

        public TypeMapper(MappingService mappingService)
        {
            _mappingService = mappingService;

            CompileNonGenericMapFunc();
        }

        #endregion

        #region Compilation phase

        public void Compile()
        {
            if (_mapFunc != null) return;

            var destVariable = GetDestionationVariable();

            ProcessAutoProperties();

            var expressions = new List<Expression> { destVariable };
            var expressionsWithDest = new List<Expression> { destVariable };

            if (_beforeMapHandler != null)
            {
                Expression<Action<T, TN>> beforeExpression = (src, dest) => _beforeMapHandler(src, dest);
                var beforeInvokeExpr = Expression.Invoke(beforeExpression, _sourceParameter, destVariable.Left);
                expressions.Add(beforeInvokeExpr);
                expressionsWithDest.Add(beforeInvokeExpr);
            }

            expressions.AddRange(_propertyCache.Values);
            expressionsWithDest.AddRange(_propertyDestInstCache.Values);

            var customProps = _customPropertyCache.Where(k => !_ignoreList.Contains(k.Key)).Select(k => k.Value);
            var customDestProps = _customPropertyDestInstCache.Where(k => !_ignoreList.Contains(k.Key)).Select(k => k.Value);
            expressions.AddRange(customProps);
            expressionsWithDest.AddRange(customDestProps);

            if (_afterMapHandler != null)
            {
                Expression<Action<T, TN>> afterExpression = (src, dest) => _afterMapHandler(src, dest);
                var afterInvokeExpr = Expression.Invoke(afterExpression, _sourceParameter, destVariable.Left);
                expressions.Add(afterInvokeExpr);
                expressionsWithDest.Add(afterInvokeExpr);
            }

            _giveAway.AddRange(expressions);
            _giveWithDestinationAway.AddRange(expressionsWithDest);

            expressions.Add(destVariable.Left);

            var variables = new List<ParameterExpression> { destVariable.Left as ParameterExpression };

            var finalExpression = Expression.Block(variables, expressions);
            var substituteParameterVisitor = new SubstituteParameterVisitor(_sourceParameter,
                destVariable.Left as ParameterExpression);
            Expression resultExpression = substituteParameterVisitor.Visit(finalExpression) as BlockExpression;

            var expression = Expression.Lambda<Func<T, TN>>(resultExpression, _sourceParameter);
            _mapFunc = expression.Compile();
        }

        public void CompileDestinationInstance()
        {
            if (_mapDestInstFunc != null) return;

            ProcessAutoProperties();

            var expressions = new List<Expression>();

            if (_beforeMapHandler != null)
            {
                Expression<Action<T, TN>> beforeExpression = (src, dest) => _beforeMapHandler(src, dest);
                var beforeInvokeExpr = Expression.Invoke(beforeExpression, _sourceParameter, _destFakeParameter);
                expressions.Add(beforeInvokeExpr);
            }

            expressions.AddRange(_propertyDestInstCache.Values);

            var customProps = _customPropertyDestInstCache.Where(k => !_ignoreList.Contains(k.Key)).Select(k => k.Value);
            expressions.AddRange(customProps);

            if (_afterMapHandler != null)
            {
                Expression<Action<T, TN>> afterExpression = (src, dest) => _afterMapHandler(src, dest);
                var afterInvokeExpr = Expression.Invoke(afterExpression, _sourceParameter, _destFakeParameter);
                expressions.Add(afterInvokeExpr);
            }

            expressions.Add(_destFakeParameter);

            var variables = new List<ParameterExpression>();

            var finalExpression = Expression.Block(variables, expressions);
            var substituteParameterVisitor = new SubstituteParameterVisitor(_sourceParameter, _destFakeParameter);
            var resultExpression = substituteParameterVisitor.Visit(finalExpression) as BlockExpression;

            var expression = Expression.Lambda<Func<T, TN, TN>>(resultExpression, _sourceParameter, _destFakeParameter);
            _mapDestInstFunc = expression.Compile();
        }

        #endregion

        #region ITypeMapper<T, TN>, ITypeMapper implementation

        public Func<object, object> GetNonGenericMapFunc()
        {
            CompileNonGenericMapFunc();
            return _nonGenericMapFunc;
        }

        public List<Expression> GetMapExpressions(bool withDestinationInstance)
        {
            Compile();
            return withDestinationInstance ? _giveWithDestinationAway : _giveAway;
        }

        public void AfterMap(Action<T, TN> afterMap)
        {
            _afterMapHandler = afterMap;
        }

        private BinaryExpression GetDestionationVariable()
        {
            var parameterExpression = Expression.Variable(typeof(TN), "dest");
            if (_constructorFunc != null)
            {
                Expression<Func<T, TN>> customConstruct = t => _constructorFunc(t);
                var invocationExpression = Expression.Invoke(customConstruct, _sourceParameter);
                return Expression.Assign(parameterExpression, invocationExpression);
            }
            var createDestination = Expression.New(typeof(TN));
            return Expression.Assign(parameterExpression, createDestination);
        }

        private void AutoMapProperty(PropertyInfo propertyGet, PropertyInfo propertySet)
        {
            var callSetPropMethod = Expression.Property(_destFakeParameter, propertySet);
            var callGetPropMethod = Expression.Property(_sourceParameter, propertyGet);

            MapMember(callSetPropMethod, callGetPropMethod);
        }

        public void MapMember<TSourceMember, TDestMember>(Expression<Func<TN, TDestMember>> left, Expression<Func<T, TSourceMember>> right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }

            if (right == null)
            {
                throw new ArgumentNullException("right");
            }

            MapMember(left.Body as MemberExpression, right.Body);
        }

        private void MapMember(MemberExpression left, Expression right)
        {
            var mappingExpression = _mappingService.GetMemberMappingExpression(left, right);

            _customPropertyCache[left.Member.Name] = mappingExpression.Item1;
            _customPropertyDestInstCache[left.Member.Name] = mappingExpression.Item2;
        }

        public void MapFunction<TMember, TNMember>(Expression<Func<TN, TNMember>> left, Func<T, TMember> right)
        {
            var memberExpression = left.Body as MemberExpression;
            Expression<Func<T, TMember>> expr = (t) => right(t);

            var parameterExpression = Expression.Parameter(typeof(T));
            var rightExpression = Expression.Invoke(expr, parameterExpression);
            if (typeof(TNMember) != typeof(TMember))
            {
                var mapComplexResult = _mappingService.GetDifferentTypeMemberMappingExpression(rightExpression, left.Body as MemberExpression);
                _customPropertyCache[memberExpression.Member.Name] = mapComplexResult.Item1;
                _customPropertyDestInstCache[memberExpression.Member.Name] = mapComplexResult.Item2;
            }
            else
            {
                var binaryExpression = Expression.Assign(memberExpression, rightExpression);
                _customPropertyCache.Add(memberExpression.Member.Name, binaryExpression);
                _customPropertyDestInstCache.Add(memberExpression.Member.Name, binaryExpression);
            }
        }

        public void Instantiate(Func<T, TN> constructor)
        {
            _constructorFunc = constructor;
        }

        public void BeforeMap(Action<T, TN> beforeMap)
        {
            _beforeMapHandler = beforeMap;
        }

        public void Ignore<TMember>(Expression<Func<TN, TMember>> left)
        {
            var memberExpression = left.Body as MemberExpression;
            _ignoreList.Add(memberExpression.Member.Name);
        }

        public TN MapTo(T src)
        {
            if (_mapFunc == null)
            {
                Compile();
            }
            return _mapFunc(src);
        }

        public TN MapTo(T src, TN dest)
        {
            if (_mapDestInstFunc == null)
            {
                CompileDestinationInstance();
            }
            return _mapDestInstFunc(src, dest);
        }

        #endregion

        #region Helpers

        private void CompileNonGenericMapFunc()
        {
            var parameterExpression = Expression.Parameter(typeof(object), "src");
            var srcConverted = Expression.Convert(parameterExpression, typeof(T));
            var srcTypedExp = Expression.Variable(typeof(T), "srcTyped");
            var srcAssigned = Expression.Assign(srcTypedExp, srcConverted);

            var customGenericType = typeof(ITypeMapper<,>).MakeGenericType(typeof(T), typeof(TN));
            var castToCustomGeneric = Expression.Convert(Expression.Constant((ITypeMapper)this), customGenericType);
            var genVariable = Expression.Variable(customGenericType);
            var assignExp = Expression.Assign(genVariable, castToCustomGeneric);
            var methodInfo = customGenericType.GetMethod("MapTo", new[] { typeof(T) });

            var mapCall = Expression.Call(genVariable, methodInfo, srcTypedExp);
            var resultVarExp = Expression.Variable(typeof(object), "result");
            var convertToObj = Expression.Convert(mapCall, typeof(object));
            var assignResult = Expression.Assign(resultVarExp, convertToObj);

            var blockExpression = Expression.Block(new[] { srcTypedExp, genVariable, resultVarExp }, new Expression[] { srcAssigned, assignExp, assignResult, resultVarExp });
            var lambda = Expression.Lambda<Func<object, object>>(blockExpression, parameterExpression);
            //var lambda = Expression.Lambda<Func<object, object>>(mapCall, srcTypedExp, genVariable, parameterExpression);
            _nonGenericMapFunc = lambda.Compile();
        }

        private void ProcessAutoProperties()
        {
            var getProps =
                typeof(T).GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);
            var setProps =
                typeof(TN).GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);

            foreach (var prop in getProps)
            {
                if (_ignoreList.Contains(prop.Name) || _customPropertyCache.ContainsKey(prop.Name))
                {
                    continue;
                }
                var setprop = setProps.FirstOrDefault(x => String.Equals(x.Name, prop.Name, StringComparison.OrdinalIgnoreCase));

                if (setprop == null || !setprop.CanWrite || !setprop.GetSetMethod(true).IsPublic)
                {
                    _ignoreList.Add(prop.Name);
                    continue;
                }

                AutoMapProperty(prop, setprop);
            }
        }

        #endregion
    }
}

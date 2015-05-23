using System;
using System.Collections;
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
        private readonly ParameterExpression _sourceParameter = Expression.Parameter(typeof(T), "src");
        private readonly ParameterExpression _destFakeParameter = Expression.Parameter(typeof(TN), "dest");
        private BinaryExpression _destVariable;
        private Func<T, TN> _mapFunc;
        private readonly List<string> _ignoreList = new List<string>(); 
        private readonly Dictionary<string, Expression> _propertyCache = new Dictionary<string, Expression>();
        private readonly Dictionary<string, Expression> _customPropertyCache = new Dictionary<string, Expression>();

        private ICustomTypeMapper<T, TN> _customTypeMapper;
        private Action<T, TN> _beforeMapHandler;
        private Action<T, TN> _afterMapHandler;
        private Func<T, TN> _constructorFunc;
        private BlockExpression _finalExpression;

        private Func<object, object> _nonGenericMapFunc; 

        #endregion

        #region Constructors

        public TypeMapper()
        {
            CompileNonGenericMapFunc();
        }

        #endregion

        #region Compilation phase

        public void Compile()
        {
            if (_mapFunc != null) return;

            Expression resultExpression;
            if (_customTypeMapper != null)
            {
                resultExpression = CompileCustomType();
            }
            else
            {
                _destVariable = GetDestionationVariable();

                ProcessAutoProperties();

                var expressions = new List<Expression>
                {
                    _destVariable
                };

                if (_beforeMapHandler != null)
                {
                    Expression<Action<T, TN>> beforeExpression = (src, dest) => _beforeMapHandler(src, dest);
                    var beforeInvokeExpr = Expression.Invoke(beforeExpression, _sourceParameter, _destVariable.Left);
                    expressions.Add(beforeInvokeExpr);
                }

                expressions.AddRange(_propertyCache.Values);

                var customProps = _customPropertyCache.Where(k => !_ignoreList.Contains(k.Key)).Select(k => k.Value);
                expressions.AddRange(customProps);

                _giveAway.AddRange(expressions);

                if (_afterMapHandler != null)
                {
                    Expression<Action<T, TN>> afterExpression = (src, dest) => _afterMapHandler(src, dest);
                    var afterInvokeExpr = Expression.Invoke(afterExpression, _sourceParameter, _destVariable.Left);
                    expressions.Add(afterInvokeExpr);
                }

                expressions.Add(_destVariable.Left);

                var variables = new List<ParameterExpression> {_destVariable.Left as ParameterExpression};

                _finalExpression = Expression.Block(variables, expressions);
                var substituteParameterVisitor = new SubstituteParameterVisitor(_sourceParameter,
                    _destVariable.Left as ParameterExpression);
                resultExpression = substituteParameterVisitor.Visit(_finalExpression) as BlockExpression;
            }

            var expression = Expression.Lambda<Func<T,TN>>(resultExpression, _sourceParameter);
            _mapFunc = expression.Compile();
        }

        private Expression CompileCustomType()
        {
            Expression<Func<T, TN>> customMapper = src => _customTypeMapper.Map(src);
            var invocationExpression = Expression.Invoke(customMapper, _sourceParameter);
            var parameterExpression = Expression.Variable(typeof (TN), "dest");
            var binaryExpression = Expression.Assign(parameterExpression, invocationExpression);
            _giveAway.Add(invocationExpression);
            _giveAway.Add(binaryExpression);
            var resultExpression = Expression.Block(new[] {parameterExpression}, _giveAway);
            return resultExpression;
        }

        #endregion

        #region ITypeMapper<T, TN>, ITypeMapper implementation

        public Func<object, object> GetNonGenericMapFunc()
        {
            return _nonGenericMapFunc;
        }

        public List<Expression> GetMapExpressions()
        {
            Compile();
            return _giveAway;
        }

        public IList ProcessCollection(IEnumerable src)
        {
            var source = src as IEnumerable<T>;
            var destination = new List<TN>(source.Count());
            foreach (var item in source)
            {
                destination.Add(MapTo(item));
            }
            return destination;
        }

        public IEnumerable ProcessArray(IEnumerable src)
        {
            var source = src as T[];
            var destination = new List<TN>(source.Length);
            for (var i = 0; i < source.Length; i++)
            {
                destination.Add(MapTo(source[i]));
            }
            return destination.ToArray();
        }

        public IQueryable ProcessQueryable(IEnumerable src)
        {
            var source = src as IEnumerable<T>;
            var destination = new List<TN>(source.Count());
            foreach (var item in source)
            {
                destination.Add(MapTo(item));
            }
            return destination.AsQueryable();
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

        public void AutoMapProperty(PropertyInfo propertyGet, PropertyInfo propertySet)
        {
            var callGetPropMethod = Expression.Property(_sourceParameter, propertyGet);
            var callSetPropMethod = Expression.Property(_destFakeParameter, propertySet);
            if (!_propertyCache.ContainsKey(propertySet.Name))
            {
                if (propertySet.PropertyType != propertyGet.PropertyType)
                {
                    var mapComplexResult = MapDifferentTypeProps(propertyGet.PropertyType, propertySet.PropertyType, callGetPropMethod, callSetPropMethod);
                    _propertyCache[propertySet.Name] = mapComplexResult;
                }
                else
                {
                    var assignExp = Expression.Assign(callSetPropMethod, callGetPropMethod);
                    _propertyCache[propertySet.Name] = assignExp;
                }
            }
        }

        public void MapMember<TMember, TNMember>(Expression<Func<TN, TNMember>> left, Expression<Func<T, TMember>> right)
        {
            var memberExpression = left.Body as MemberExpression;

            if (typeof(TNMember) != typeof(TMember))
            {
                var mapComplexResult = MapDifferentTypeProps(typeof(TMember), typeof(TNMember), right.Body, left.Body as MemberExpression);
                _customPropertyCache[memberExpression.Member.Name] = mapComplexResult;
            }
            else
            {
                var binaryExpression = Expression.Assign(memberExpression, right.Body);
                _customPropertyCache.Add(memberExpression.Member.Name, binaryExpression);
            }
        }

        public void MapFunction<TMember, TNMember>(Expression<Func<TN, TNMember>> left, Func<T, TMember> right)
        {
            var memberExpression = left.Body as MemberExpression;
            Expression<Func<T, TMember>> expr = (t) => right(t);

            var parameterExpression = Expression.Parameter(typeof(T));
            var rightExpression = Expression.Invoke(expr, parameterExpression);
            if (typeof(TNMember) != typeof(TMember))
            {
                var mapComplexResult = MapDifferentTypeProps(typeof(TMember), typeof(TNMember), rightExpression, left.Body as MemberExpression);
                _customPropertyCache[memberExpression.Member.Name] = mapComplexResult;
            }
            else
            {
                var binaryExpression = Expression.Assign(memberExpression, rightExpression);
                _customPropertyCache.Add(memberExpression.Member.Name, binaryExpression);
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

        public void Custom(ICustomTypeMapper<T, TN> customTypeMapper)
        {
            _customTypeMapper = customTypeMapper;
        }

        public TN MapTo(T obj)
        {
            if (_mapFunc == null)
            {
                Compile();
            }
            return _mapFunc(obj);
        }

        #endregion

        #region Helpers

        private void CompileNonGenericMapFunc()
        {
            var parameterExpression = Expression.Parameter(typeof(object), "src");
            var srcConverted = Expression.Convert(parameterExpression, typeof(T));
            var srcTypedExp = Expression.Variable(typeof(T), "srcTyped");
            var srcAssigned = Expression.Assign(srcConverted, srcTypedExp);

            var customGenericType = typeof(ITypeMapper<,>).MakeGenericType(typeof(T), typeof(TN));
            var castToCustomGeneric = Expression.Convert(Expression.Constant((ITypeMapper)this), customGenericType);
            var genVariable = Expression.Variable(customGenericType);
            var assignExp = Expression.Assign(genVariable, castToCustomGeneric);
            var methodInfo = customGenericType.GetMethod("MapTo");

            var mapCall = Expression.Call(genVariable, methodInfo, srcTypedExp);
            var lambda = Expression.Lambda<Func<object, object>>(mapCall, parameterExpression, srcTypedExp, genVariable);
            _nonGenericMapFunc = lambda.Compile();
        }

        private void ProcessAutoProperties()
        {
            var getProps =
                typeof(T).GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);
            var setProps =
                typeof(TN).GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
                    .ToDictionary((k => k.Name), (v => v));

            var gets = new List<PropertyInfo>();
            var sets = new List<PropertyInfo>();

            foreach (var prop in getProps)
            {
                if (_ignoreList.Contains(prop.Name) || !setProps.ContainsKey(prop.Name) || _customPropertyCache.ContainsKey(prop.Name)) continue;
                var setprop = setProps[prop.Name];
                if (!(setprop.CanWrite && setprop.GetSetMethod(true).IsPublic))
                {
                    _ignoreList.Add(prop.Name);
                    continue;
                }

                gets.Add(prop);
                sets.Add(setprop);
            }

            for (var i = 0; i < gets.Count; i++)
            {
                AutoMapProperty(gets[i], sets[i]);
            }
        }

        private Expression MapDifferentTypeProps(Type sourceType, Type destType, Expression callGetPropMethod, MemberExpression callSetPropMethod)
        {
            var tCol =
                sourceType.GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                (sourceType.IsGenericType
                    && sourceType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? sourceType
                    : null);

            var tnCol = destType.GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                        (destType.IsGenericType && destType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? destType
                            : null);

            var blockExpression = (tCol != null && tnCol != null)
                ? MapCollection(sourceType, destType, tCol, tnCol, callGetPropMethod, callSetPropMethod)
                : MapProperty(sourceType, destType, callGetPropMethod, callSetPropMethod);


            var refSrcType = sourceType.IsClass;
            var destPropType = destType;
            if (refSrcType)
            {
                var resultExpression =
                    Expression.IfThenElse(Expression.Equal(callGetPropMethod, Expression.Constant(null)),
                        Expression.Assign(callSetPropMethod, Expression.Default(destPropType)), blockExpression);
                return resultExpression;
            }
            return blockExpression;
        }

        private static BlockExpression MapCollection(Type sourcePropType, Type destpropType, Type tCol, Type tnCol, Expression callGetPropMethod, MemberExpression callSetPropMethod)
        {
            var sourceType = tCol.GetGenericArguments()[0];
            var destType = tnCol.GetGenericArguments()[0];
            var sourceVariable = Expression.Variable(sourcePropType,
                string.Format("{0}_{1}", typeof(T).Name, Guid.NewGuid().ToString().Replace("-", "_")));
            var assignSourceFromProp = Expression.Assign(sourceVariable, callGetPropMethod);

            var destList = typeof(List<>).MakeGenericType(destType);
            var destColl = Expression.Variable(destList, string.Format("{0}_{1}", typeof(TN).Name, callSetPropMethod.Member.Name));

            var constructorInfo = destList.GetConstructors().First(c => c.GetParameters().FirstOrDefault(p => p.ParameterType == typeof(int)) != null);

            var srcCountExp = Expression.Call(typeof (Enumerable), "Count", new[] {sourceType}, sourceVariable);

            var newColl = Expression.New(constructorInfo, srcCountExp);
            var destAssign = Expression.Assign(destColl, newColl);

            var closedEnumeratorSourceType = typeof(IEnumerator<>).MakeGenericType(sourceType);
            var closedEnumerableSourceType = typeof(IEnumerable<>).MakeGenericType(sourceType);
            var enumerator = Expression.Variable(closedEnumeratorSourceType,
                string.Format("{0}_{1}Enum", typeof(T).Name, Guid.NewGuid().ToString().Replace("-", "_")));
            var assignToEnum = Expression.Assign(enumerator,
                Expression.Call(sourceVariable, closedEnumerableSourceType.GetMethod("GetEnumerator")));
            var doMoveNext = Expression.Call(enumerator, typeof(IEnumerator).GetMethod("MoveNext"));

            var current = Expression.Property(enumerator, "Current");
            var sourceColItmVariable = Expression.Variable(sourceType,
                string.Format("{0}_{1}ItmSrc", typeof(T).Name, Guid.NewGuid().ToString().Replace("-", "_")));
            var assignSourceItmFromProp = Expression.Assign(sourceColItmVariable, current);

            var destColItmVariable = Expression.Variable(destType,
                string.Format("{0}_{1}ItmDest", typeof(TN).Name, Guid.NewGuid().ToString().Replace("-", "_")));
            var mapExprForType = Mapper.GetMapExpressions(sourceType, destType);
            var blockForSubstiyution = Expression.Block(mapExprForType);
            var substBlock =
                new SubstituteParameterVisitor(sourceColItmVariable, destColItmVariable).Visit(
                    blockForSubstiyution) as BlockExpression;
            var resultMapExprForType = substBlock.Expressions;

            var addToNewColl = Expression.Call(destColl, "Add", null, destColItmVariable);
            var blockExps = new List<Expression> { assignSourceItmFromProp };
            blockExps.AddRange(resultMapExprForType);
            blockExps.Add(addToNewColl);

            var ifTrueBlock = Expression.Block(new[] { sourceColItmVariable, destColItmVariable }, blockExps);

            var brk = Expression.Label();
            var loopExpression = Expression.Loop(
                Expression.IfThenElse(
                    Expression.NotEqual(doMoveNext, Expression.Constant(false)),
                    ifTrueBlock
                    , Expression.Break(brk))
                , brk);

            Expression resultCollection = destColl;
            if (destpropType.IsArray)
            {
                resultCollection = Expression.Call(destColl, destList.GetMethod("ToArray"));
            }
            else
            {
                if (destpropType.IsGenericType && destpropType.GetInterfaces().Any(t => t == typeof(IQueryable)))
                {
                    resultCollection = Expression.Call(typeof(Queryable), "AsQueryable", new[] { destType }, destColl);
                }
            }

            var assignResult = Expression.Assign(callSetPropMethod, resultCollection);

            var parameters = new List<ParameterExpression> { sourceVariable, destColl, enumerator };
            var expressions = new List<Expression>
            {
                assignSourceFromProp,
                destAssign,
                assignToEnum,
                loopExpression,
                assignResult
            };

            var blockExpression = Expression.Block(parameters, expressions);
            return blockExpression;
        }

        private static BlockExpression MapProperty(Type sourceType, Type destType, Expression callGetPropMethod, MemberExpression callSetPropMethod)
        {
            var sourceVariable = Expression.Variable(sourceType,
                string.Format("{0}_{1}Src", typeof(T).Name, Guid.NewGuid().ToString().Replace("-", "_")));
            var assignSourceFromProp = Expression.Assign(sourceVariable, callGetPropMethod);
            var mapExprForType = Mapper.GetMapExpressions(sourceType, destType);
            var destVariable = Expression.Variable(destType,
                string.Format("{0}_{1}_{2}Dest", typeof(TN).Name, callSetPropMethod.Member.Name,
                    Guid.NewGuid().ToString().Replace("-", "_")));
            var blockForSubstiyution = Expression.Block(mapExprForType);
            var substBlock =
                new SubstituteParameterVisitor(sourceVariable, destVariable).Visit(blockForSubstiyution) as
                    BlockExpression;
            var resultMapExprForType = substBlock.Expressions;

            var assignExp = Expression.Assign(callSetPropMethod, destVariable);

            var expressions = new List<Expression>();
            expressions.Add(assignSourceFromProp);
            expressions.AddRange(resultMapExprForType);
            expressions.Add(assignExp);

            var parameterExpressions = new List<ParameterExpression> { sourceVariable, destVariable };
            var blockExpression = Expression.Block(parameterExpressions, expressions);

            return blockExpression;
        }

        #endregion
    }
}

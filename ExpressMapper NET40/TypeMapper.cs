using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private BinaryExpression _destVariable;
        private Func<T, TN> _mapFunc;
        private Func<T, TN, TN> _mapDestInstFunc;
        private readonly List<string> _ignoreList = new List<string>();
        private readonly Dictionary<string, Expression> _propertyCache = new Dictionary<string, Expression>();
        private readonly Dictionary<string, Expression> _customPropertyCache = new Dictionary<string, Expression>();

        private readonly Dictionary<string, Expression> _propertyDestInstCache = new Dictionary<string, Expression>();
        private readonly Dictionary<string, Expression> _customPropertyDestInstCache = new Dictionary<string, Expression>();

        private ICustomTypeMapper<T, TN> _customTypeMapper;
        private Action<T, TN> _beforeMapHandler;
        private Action<T, TN> _afterMapHandler;
        private Func<T, TN> _constructorFunc;
        private BlockExpression _finalExpression;

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

        public IQueryable ProcessQueryable(IEnumerable src, IQueryable dest)
        {
            throw new NotImplementedException();
        }

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

                var expressions = new List<Expression> { _destVariable };
                var expressionsWithDest = new List<Expression> { _destVariable };

                if (_beforeMapHandler != null)
                {
                    Expression<Action<T, TN>> beforeExpression = (src, dest) => _beforeMapHandler(src, dest);
                    var beforeInvokeExpr = Expression.Invoke(beforeExpression, _sourceParameter, _destVariable.Left);
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
                    var afterInvokeExpr = Expression.Invoke(afterExpression, _sourceParameter, _destVariable.Left);
                    expressions.Add(afterInvokeExpr);
                    expressionsWithDest.Add(afterInvokeExpr);
                }

                _giveAway.AddRange(expressions);
                _giveWithDestinationAway.AddRange(expressionsWithDest);

                expressions.Add(_destVariable.Left);

                var variables = new List<ParameterExpression> { _destVariable.Left as ParameterExpression };

                _finalExpression = Expression.Block(variables, expressions);
                var substituteParameterVisitor = new SubstituteParameterVisitor(_sourceParameter,
                    _destVariable.Left as ParameterExpression);
                resultExpression = substituteParameterVisitor.Visit(_finalExpression) as BlockExpression;
            }

            var expression = Expression.Lambda<Func<T, TN>>(resultExpression, _sourceParameter);
            _mapFunc = expression.Compile();
        }

        public void CompileDestinationInstance()
        {
            // TODO: _mappingService.IsDestinationInstance = true;

            var destVariable = Expression.Parameter(typeof(TN), "dest");

            ProcessAutoProperties();

            var expressions = new List<Expression>();

            if (_beforeMapHandler != null)
            {
                Expression<Action<T, TN>> beforeExpression = (src, dest) => _beforeMapHandler(src, dest);
                var beforeInvokeExpr = Expression.Invoke(beforeExpression, _sourceParameter, destVariable);
                expressions.Add(beforeInvokeExpr);
            }

            expressions.AddRange(_propertyDestInstCache.Values);

            var customProps = _customPropertyDestInstCache.Where(k => !_ignoreList.Contains(k.Key)).Select(k => k.Value);
            expressions.AddRange(customProps);

            if (_afterMapHandler != null)
            {
                Expression<Action<T, TN>> afterExpression = (src, dest) => _afterMapHandler(src, dest);
                var afterInvokeExpr = Expression.Invoke(afterExpression, _sourceParameter, destVariable);
                expressions.Add(afterInvokeExpr);
            }

            expressions.Add(destVariable);

            var variables = new List<ParameterExpression>();

            _finalExpression = Expression.Block(variables, expressions);
            var substituteParameterVisitor = new SubstituteParameterVisitor(_sourceParameter, destVariable);
            var resultExpression = substituteParameterVisitor.Visit(_finalExpression) as BlockExpression;

            var expression = Expression.Lambda<Func<T, TN, TN>>(resultExpression, _sourceParameter, destVariable);
            _mapDestInstFunc = expression.Compile();

            // TODO: _mappingService.IsDestinationInstance = false;
        }

        private Expression CompileCustomType()
        {
            Expression<Func<T, TN>> customMapper = src => _customTypeMapper.Map(new DefaultMappingContext<T, TN> { Source = src });
            var invocationExpression = Expression.Invoke(customMapper, _sourceParameter);
            var parameterExpression = Expression.Variable(typeof(TN), "dest");
            var binaryExpression = Expression.Assign(parameterExpression, invocationExpression);
            _giveAway.Add(invocationExpression);
            _giveAway.Add(binaryExpression);
            var resultExpression = Expression.Block(new[] { parameterExpression }, _giveAway);
            return resultExpression;
        }

        #endregion

        #region ITypeMapper<T, TN>, ITypeMapper implementation

        public Func<object, object> GetNonGenericMapFunc()
        {
            return _nonGenericMapFunc;
        }

        public List<Expression> GetMapExpressions(bool withDestinationInstance = false)
        {
            Compile();
            return withDestinationInstance ? _giveWithDestinationAway : _giveAway;
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

        public IList ProcessCollection(IEnumerable src, IList dest)
        {
            throw new NotImplementedException();
        }

        public IEnumerable ProcessArray(IEnumerable src, IEnumerable dest)
        {
            throw new NotImplementedException();
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
                Type setPropertyNullableType = Nullable.GetUnderlyingType(propertySet.PropertyType);
                Type getPropertyNullableType = Nullable.GetUnderlyingType(propertyGet.PropertyType);

                Type setPropertyType = setPropertyNullableType == null ? propertySet.PropertyType : setPropertyNullableType;
                Type getPropertyType = getPropertyNullableType == null ? propertyGet.PropertyType : getPropertyNullableType;

                if (setPropertyType != getPropertyType)
                {
                    var customMapExpression = _mappingService.GetCustomMapExpression(getPropertyType, setPropertyType);
                    var customMapExpressionWithDest = _mappingService.GetCustomMapExpression(getPropertyType, setPropertyType, true);
                    if (customMapExpression != null && customMapExpressionWithDest != null)
                    {
                        var srcExp = Expression.Variable(getPropertyType,
                            string.Format("{0}Src", Guid.NewGuid().ToString("N")));
                        var assignSrcExp = Expression.Assign(srcExp, callGetPropMethod);

                        var destExp = Expression.Variable(setPropertyType,
                            string.Format("{0}Dest", Guid.NewGuid().ToString("N")));
                        var assignDestExp = Expression.Assign(destExp, callSetPropMethod);

                        var substituteParameterVisitor = new SubstituteParameterVisitor(srcExp, destExp);
                        var blockExpression = substituteParameterVisitor.Visit(customMapExpression) as BlockExpression;
                        var assignResultExp = Expression.Assign(callSetPropMethod, destExp);
                        var resultBlockExp = Expression.Block(new[] { srcExp, destExp }, assignSrcExp, blockExpression,
                            assignResultExp);
                        var resultBlockWithDestExp = Expression.Block(new[] { srcExp, destExp }, assignSrcExp,
                            assignDestExp, blockExpression, assignResultExp);

                        var checkNullExp =
                            Expression.IfThenElse(Expression.Equal(callGetPropMethod, Expression.Default(getPropertyType)),
                                Expression.Assign(callSetPropMethod, Expression.Default(setPropertyType)), resultBlockExp);

                        var checkNullExpWithDest =
                            Expression.IfThenElse(Expression.Equal(callGetPropMethod, Expression.Default(getPropertyType)),
                                Expression.Assign(callSetPropMethod, Expression.Default(setPropertyType)),
                                resultBlockWithDestExp);

                        var releaseExp = Expression.Block(new ParameterExpression[] { }, checkNullExp);
                        var releaseWithDestExp = Expression.Block(new ParameterExpression[] { }, checkNullExpWithDest);

                        _customPropertyCache[propertySet.Name] = releaseExp;
                        _customPropertyDestInstCache[propertySet.Name] = releaseWithDestExp;
                    }
                    else if (typeof(IConvertible).IsAssignableFrom(setPropertyType) &&
                        typeof(IConvertible).IsAssignableFrom(getPropertyType))
                    {
                        var assignExp = CreateConvertibleAssignExpression(callSetPropMethod,
                            callGetPropMethod,
                            propertySet.PropertyType,
                            propertyGet.PropertyType,
                            setPropertyNullableType);

                        _propertyCache[propertySet.Name] = assignExp;
                        _propertyDestInstCache[propertySet.Name] = assignExp;
                    }
                    else
                    {
                        var mapComplexResult = MapDifferentTypeProps(getPropertyType, setPropertyType,
                            callGetPropMethod, callSetPropMethod);
                        _propertyCache[propertySet.Name] = mapComplexResult.Item1;
                        _propertyDestInstCache[propertySet.Name] = mapComplexResult.Item2;
                    }
                }
                else
                {
                    var assignExp = CreateAssignExpression(callSetPropMethod,
                        callGetPropMethod,
                        propertySet.PropertyType,
                        setPropertyNullableType,
                        getPropertyNullableType);

                    _propertyCache[propertySet.Name] = assignExp;
                    _propertyDestInstCache[propertySet.Name] = assignExp;
                }
            }
        }

        private static Expression CreateAssignExpression(Expression setMethod, Expression getMethod, Type setType, Type setNullableType, Type getNullableType)
        {
            Expression left = setMethod;
            Expression right = getMethod;

            if (setNullableType == null && getNullableType != null)
            {
                // Nullable to non nullable map
                right = Expression.Call(getMethod, "GetValueOrDefault", Type.EmptyTypes);
            }
            else if (setNullableType != null && getNullableType == null)
            {
                // Non nullable to nullable  map
                right = Expression.Convert(getMethod, setType);
            }

            return Expression.Assign(left, right);
        }

        private static Expression CreateConvertibleAssignExpression(Expression setMethod, Expression getMethod, Type setType, Type getType, Type setNullableType)
        {
            Expression left = setMethod;
            Expression right = getMethod;

            if ((setNullableType ?? setType).IsEnum && (getType == typeof(string)))
            {
                return Expression.IfThen(
                    Expression.NotEqual(getMethod, StaticExpressions.NullConstant),
                        Expression.Assign(left,
                            Expression.Convert(
                                Expression.Call(typeof(Enum).GetMethod("Parse", new Type[] { typeof(Type), typeof(string), typeof(bool) }), Expression.Constant(setNullableType ?? setType), right, Expression.Constant(true)),
                                setType)));
            }
            else if (!getType.IsClass)
            {
                return Expression.Assign(left,
                            Expression.Convert(
                                Expression.Call(typeof(Convert).GetMethod("ChangeType", new Type[] { typeof(object), typeof(Type) }), Expression.Convert(right, typeof(object)), Expression.Constant(setNullableType ?? setType)),
                                setType));
            }
            else
            {
                return Expression.IfThen(
                    Expression.NotEqual(getMethod, StaticExpressions.NullConstant),
                        Expression.Assign(left,
                            Expression.Convert(
                                Expression.Call(typeof(Convert).GetMethod("ChangeType", new Type[] { typeof(object), typeof(Type) }), Expression.Convert(right, typeof(object)), Expression.Constant(setNullableType ?? setType)),
                                setType)));
            }
        }

        public void MapMember<TSourceMember, TDestMember>(Expression<Func<TN, TDestMember>> left, Expression<Func<T, TSourceMember>> right)
        {
            var nullCheckNestedMemberVisitor = new NullCheckNestedMemberVisitor();
            nullCheckNestedMemberVisitor.Visit(right);

            var memberExpression = left.Body as MemberExpression;

            Type destNullableType = Nullable.GetUnderlyingType(typeof(TDestMember));
            Type sourceNullableType = Nullable.GetUnderlyingType(typeof(TSourceMember));

            Type destType = destNullableType == null ? typeof(TDestMember) : destNullableType;
            Type sourceType = sourceNullableType == null ? typeof(TSourceMember) : sourceNullableType;

            if (destType != sourceType)
            {
                var customMapExpression = _mappingService.GetCustomMapExpression(typeof(TSourceMember), typeof(TDestMember));
                var customMapExpressionWithDest = _mappingService.GetCustomMapExpression(typeof(TSourceMember), typeof(TDestMember), true);
                if (customMapExpression != null && customMapExpressionWithDest != null)
                {
                    var srcExp = Expression.Variable(typeof(TSourceMember),
                        string.Format("{0}Src", Guid.NewGuid().ToString("N")));
                    var assignSrcExp = Expression.Assign(srcExp, right.Body);

                    var destExp = Expression.Variable(typeof(TDestMember),
                        string.Format("{0}Dest", Guid.NewGuid().ToString("N")));
                    var assignDestExp = Expression.Assign(destExp, left.Body);

                    var substituteParameterVisitor = new SubstituteParameterVisitor(srcExp, destExp);
                    var blockExpression = substituteParameterVisitor.Visit(customMapExpression) as BlockExpression;
                    var assignResultExp = Expression.Assign(left.Body, destExp);
                    var resultBlockExp = Expression.Block(new[] { srcExp, destExp }, assignSrcExp, blockExpression, assignResultExp);
                    var resultBlockWithDestExp = Expression.Block(new[] { srcExp, destExp }, assignSrcExp, assignDestExp, blockExpression, assignResultExp);

                    var checkNullExp =
                        Expression.IfThenElse(Expression.Equal(right.Body, Expression.Default(typeof(TSourceMember))),
                            Expression.Assign(left.Body, Expression.Default(typeof(TDestMember))), resultBlockExp);

                    var checkNullExpWithDest =
                        Expression.IfThenElse(Expression.Equal(right.Body, Expression.Default(typeof(TSourceMember))),
                            Expression.Assign(left.Body, Expression.Default(typeof(TDestMember))), resultBlockWithDestExp);

                    var releaseExp = Expression.Block(new ParameterExpression[] { }, checkNullExp);
                    var releaseWithDestExp = Expression.Block(new ParameterExpression[] { }, checkNullExpWithDest);

                    _customPropertyCache[memberExpression.Member.Name] = releaseExp;
                    _customPropertyDestInstCache[memberExpression.Member.Name] = releaseWithDestExp;
                }
                else if (typeof(IConvertible).IsAssignableFrom(destType) &&
                    typeof(IConvertible).IsAssignableFrom(sourceType))
                {
                    var assignExp = CreateConvertibleAssignExpression(memberExpression,
                        right.Body,
                        typeof(TDestMember),
                        sourceType,
                        destNullableType);

                    _customPropertyCache[memberExpression.Member.Name] = assignExp;
                    _customPropertyDestInstCache[memberExpression.Member.Name] = assignExp;
                }
                else
                {
                    var mapComplexResult = MapDifferentTypeProps(typeof(TSourceMember), typeof(TDestMember), right.Body, left.Body as MemberExpression);

                    _customPropertyCache[memberExpression.Member.Name] =
                        nullCheckNestedMemberVisitor.CheckNullExpression != null
                            ? Expression.Condition(nullCheckNestedMemberVisitor.CheckNullExpression,
                                Expression.Assign(memberExpression, Expression.Default(left.Body.Type)),
                                mapComplexResult.Item1)
                            : mapComplexResult.Item1;
                    _customPropertyDestInstCache[memberExpression.Member.Name] =
                        nullCheckNestedMemberVisitor.CheckNullExpression != null
                            ? Expression.Condition(nullCheckNestedMemberVisitor.CheckNullExpression,
                                Expression.Assign(memberExpression, Expression.Default(left.Body.Type)),
                                mapComplexResult.Item2)
                            : mapComplexResult.Item2;
                }
            }
            else
            {
                var binaryExpression = CreateAssignExpression(memberExpression,
                    right.Body,
                    typeof(TDestMember),
                    destNullableType,
                    sourceNullableType);

                var conditionalExpression = nullCheckNestedMemberVisitor.CheckNullExpression != null ? Expression.Condition(nullCheckNestedMemberVisitor.CheckNullExpression, Expression.Assign(memberExpression, Expression.Default(left.Body.Type)), binaryExpression) : (Expression)binaryExpression;
                _customPropertyCache[memberExpression.Member.Name] = conditionalExpression;
                _customPropertyDestInstCache[memberExpression.Member.Name] = conditionalExpression;
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

        public void Custom(ICustomTypeMapper<T, TN> customTypeMapper)
        {
            _customTypeMapper = customTypeMapper;
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

        private Tuple<Expression, Expression> MapDifferentTypeProps(Type sourceType, Type destType, Expression callGetPropMethod, MemberExpression callSetPropMethod)
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
                ? new Tuple<Expression, Expression>(MapCollection(sourceType, destType, tCol, tnCol, callGetPropMethod, callSetPropMethod), MapCollection2(sourceType, destType, tCol, tnCol, callGetPropMethod, callSetPropMethod))
                : new Tuple<Expression, Expression>(MapProperty(sourceType, destType, callGetPropMethod, callSetPropMethod), MapProperty2(sourceType, destType, callGetPropMethod, callSetPropMethod));


            var refSrcType = sourceType.IsClass;
            var destPropType = destType;
            if (refSrcType)
            {
                var resultExpression =
                    new Tuple<Expression, Expression>(
                        Expression.IfThenElse(Expression.Equal(callGetPropMethod, StaticExpressions.NullConstant),
                            Expression.Assign(callSetPropMethod, Expression.Default(destPropType)),
                            blockExpression.Item1),
                        Expression.IfThenElse(Expression.Equal(callGetPropMethod, StaticExpressions.NullConstant),
                            Expression.Assign(callSetPropMethod, Expression.Default(destPropType)),
                            blockExpression.Item2));
                return resultExpression;
            }
            return blockExpression;
        }

        private BlockExpression MapCollection(Type sourcePropType, Type destpropType, Type tCol, Type tnCol, Expression callGetPropMethod, MemberExpression callSetPropMethod)
        {
            var sourceType = tCol.GetGenericArguments()[0];
            var destType = tnCol.GetGenericArguments()[0];
            var sourceVariable = Expression.Variable(sourcePropType,
                string.Format("{0}_{1}", typeof(T).Name, Guid.NewGuid().ToString().Replace("-", "_")));
            var assignSourceFromProp = Expression.Assign(sourceVariable, callGetPropMethod);

            var destList = typeof(List<>).MakeGenericType(destType);
            var destColl = Expression.Variable(destList, string.Format("{0}_{1}", typeof(TN).Name, callSetPropMethod.Member.Name));

            var newColl = Expression.New(destList);
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

            var loopExpression = _mappingService.CollectionLoopExpression(sourceType, destType, destColl, sourceColItmVariable, destColItmVariable,
                assignSourceItmFromProp, doMoveNext);

            Expression resultCollection = _mappingService.ConvertCollection(destpropType, destList, destType, destColl);

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

            var checkSrcForNullExp =
                Expression.IfThenElse(Expression.Equal(callGetPropMethod, StaticExpressions.NullConstant),
                    Expression.Assign(callSetPropMethod, Expression.Default(callSetPropMethod.Type)), blockExpression);
            var blockResultExp = Expression.Block(new ParameterExpression[] { }, new Expression[] { checkSrcForNullExp });

            return blockResultExp;
        }

        private BlockExpression MapCollection2(Type sourcePropType, Type destpropType, Type tCol, Type tnCol, Expression callGetPropMethod, MemberExpression callSetPropMethod)
        {
            var sourceType = tCol.GetGenericArguments()[0];
            var destType = tnCol.GetGenericArguments()[0];
            var sourceVariable = Expression.Variable(sourcePropType,
                string.Format("{0}_{1}", typeof(T).Name, Guid.NewGuid().ToString().Replace("-", "_")));
            var assignSourceVarExp = Expression.Assign(sourceVariable, callGetPropMethod);

            var srcCount = Expression.Call(typeof(Enumerable), "Count", new[] { sourceType }, sourceVariable);
            var destCount = Expression.Call(typeof(Enumerable), "Count", new[] { destType }, callSetPropMethod);

            var conditionToCreateList = Expression.NotEqual(srcCount, destCount);
            var notNullCondition = Expression.IfThenElse(conditionToCreateList,
                _mappingService.MapCollectionNotCountEquals(sourcePropType, destpropType, callGetPropMethod,
                    callSetPropMethod),
                MapCollectionCountEquals(sourcePropType, destpropType, tCol, tnCol, callGetPropMethod, callSetPropMethod));

            var result = Expression.IfThenElse(Expression.NotEqual(callSetPropMethod, StaticExpressions.NullConstant), notNullCondition,
                MapCollection(sourcePropType, destpropType, tCol, tnCol, callGetPropMethod, callSetPropMethod));

            var blockExpression = Expression.Block(new ParameterExpression[] { }, new Expression[] { result });
            var expression = new SubstituteParameterVisitor(sourceVariable).Visit(blockExpression) as BlockExpression;

            var expressions = new List<Expression> { assignSourceVarExp, expression };

            var resultExpression = Expression.Block(new[] { sourceVariable }, expressions);

            var checkSrcForNullExp =
                Expression.IfThenElse(Expression.Equal(callGetPropMethod, StaticExpressions.NullConstant),
                    Expression.Assign(callSetPropMethod, Expression.Default(callSetPropMethod.Type)), resultExpression);
            var block = Expression.Block(new ParameterExpression[] { }, new Expression[] { checkSrcForNullExp });

            return block;
        }

        private BlockExpression MapCollectionCountEquals(Type sourcePropType, Type destpropType, Type tCol, Type tnCol, Expression callGetPropMethod, MemberExpression callSetPropMethod)
        {
            var sourceType = tCol.GetGenericArguments()[0];
            var destType = tnCol.GetGenericArguments()[0];
            var sourceVariable = Expression.Variable(sourcePropType,
                string.Format("{0}_{1}", typeof(T).Name, Guid.NewGuid().ToString().Replace("-", "_")));

            // Source enumeration
            var closedEnumeratorSourceType = typeof(IEnumerator<>).MakeGenericType(sourceType);
            var closedEnumerableSourceType = typeof(IEnumerable<>).MakeGenericType(sourceType);
            var enumeratorSrc = Expression.Variable(closedEnumeratorSourceType,
                string.Format("{0}_{1}EnumSrc", typeof(T).Name, Guid.NewGuid().ToString().Replace("-", "_")));
            var assignToEnumSrc = Expression.Assign(enumeratorSrc,
                Expression.Call(sourceVariable, closedEnumerableSourceType.GetMethod("GetEnumerator")));
            var doMoveNextSrc = Expression.Call(enumeratorSrc, typeof(IEnumerator).GetMethod("MoveNext"));
            var currentSrc = Expression.Property(enumeratorSrc, "Current");
            var srcItmVarExp = Expression.Variable(sourceType,
                string.Format("{0}_{1}ItmSrc", typeof(T).Name, Guid.NewGuid().ToString().Replace("-", "_")));
            var assignSourceItmFromProp = Expression.Assign(srcItmVarExp, currentSrc);

            // dest enumeration
            var closedEnumeratorDestType = typeof(IEnumerator<>).MakeGenericType(destType);
            var closedEnumerableDestType = typeof(IEnumerable<>).MakeGenericType(destType);
            var enumeratorDest = Expression.Variable(closedEnumeratorDestType,
                string.Format("{0}_{1}EnumDest", typeof(TN).Name, Guid.NewGuid().ToString().Replace("-", "_")));
            var assignToEnumDest = Expression.Assign(enumeratorDest,
                Expression.Call(callSetPropMethod, closedEnumerableDestType.GetMethod("GetEnumerator")));
            var doMoveNextDest = Expression.Call(enumeratorDest, typeof(IEnumerator).GetMethod("MoveNext"));
            var currentDest = Expression.Property(enumeratorDest, "Current");
            var destItmVarExp = Expression.Variable(destType,
                string.Format("{0}_{1}ItmDest", typeof(T).Name, Guid.NewGuid().ToString().Replace("-", "_")));
            var assignDestItmFromProp = Expression.Assign(destItmVarExp, currentDest);

            var blockForSubstitution = _mappingService.GetCustomMapExpression(sourceType, destType, true);
            if (blockForSubstitution == null)
            {
                var mapExprForType = new List<Expression>(_mappingService.GetMapExpressions(sourceType, destType, true));

                var newDestInstanceExp = mapExprForType[0] as BinaryExpression;
                if (newDestInstanceExp != null)
                {
                    mapExprForType.RemoveAt(0);

                    var destCondition = Expression.IfThen(Expression.Equal(destItmVarExp, StaticExpressions.NullConstant),
                        newDestInstanceExp);
                    mapExprForType.Insert(0, destCondition);
                }

                blockForSubstitution = Expression.Block(mapExprForType);
            }

            var substBlock =
                new SubstituteParameterVisitor(srcItmVarExp, destItmVarExp).Visit(
                    blockForSubstitution) as BlockExpression;

            var blockExps = new List<Expression> { assignSourceItmFromProp, assignDestItmFromProp };
            blockExps.AddRange(substBlock.Expressions);

            var ifTrueBlock = Expression.Block(new[] { srcItmVarExp, destItmVarExp }, blockExps);

            var brk = Expression.Label();
            var loopExpression = Expression.Loop(
                Expression.IfThenElse(
                    Expression.AndAlso(Expression.NotEqual(doMoveNextSrc, StaticExpressions.FalseConstant), Expression.NotEqual(doMoveNextDest, StaticExpressions.FalseConstant)),
                    ifTrueBlock
                    , Expression.Break(brk))
                , brk);

            var parameters = new List<ParameterExpression> { enumeratorSrc, enumeratorDest };
            var expressions = new List<Expression>
            {
                assignToEnumSrc,
                assignToEnumDest,
                loopExpression
            };

            var blockExpression = Expression.Block(parameters, expressions);
            return blockExpression;
        }

        private BlockExpression MapProperty(Type sourceType, Type destType, Expression callGetPropMethod, MemberExpression callSetPropMethod)
        {
            var sourceVariable = Expression.Variable(sourceType,
                string.Format("{0}_{1}Src", typeof(T).Name, Guid.NewGuid().ToString().Replace("-", "_")));
            var assignSourceFromProp = Expression.Assign(sourceVariable, callGetPropMethod);
            var mapExprForType = _mappingService.GetMapExpressions(sourceType, destType);
            var destVariable = Expression.Variable(destType,
                string.Format("{0}_{1}_{2}Dest", typeof(TN).Name, callSetPropMethod.Member.Name,
                    Guid.NewGuid().ToString().Replace("-", "_")));
            var blockForSubstitution = Expression.Block(mapExprForType);
            var substBlock =
                new SubstituteParameterVisitor(sourceVariable, destVariable).Visit(blockForSubstitution) as
                    BlockExpression;
            var resultMapExprForType = substBlock.Expressions;

            var assignExp = Expression.Assign(callSetPropMethod, destVariable);

            var expressions = new List<Expression> { assignSourceFromProp };
            expressions.AddRange(resultMapExprForType);
            expressions.Add(assignExp);

            var parameterExpressions = new List<ParameterExpression> { sourceVariable, destVariable };
            var blockExpression = Expression.Block(parameterExpressions, expressions);

            return blockExpression;
        }

        private BlockExpression MapProperty2(Type sourceType, Type destType, Expression callGetPropMethod, MemberExpression callSetPropMethod)
        {
            var sourceVariable = Expression.Variable(sourceType,
                string.Format("{0}_{1}Src", typeof(T).Name, Guid.NewGuid().ToString().Replace("-", "_")));

            var assignSourceFromProp = Expression.Assign(sourceVariable, callGetPropMethod);
            var mapExprForType = new List<Expression>(_mappingService.GetMapExpressions(sourceType, destType, true));
            var destVariable = Expression.Variable(destType,
                string.Format("{0}_{1}_{2}Dest", typeof(TN).Name, callSetPropMethod.Member.Name,
                    Guid.NewGuid().ToString().Replace("-", "_")));

            var ifDestNull = destType.IsPrimitive || destType.IsEnum ? (Expression)StaticExpressions.FalseConstant : Expression.Equal(callSetPropMethod, StaticExpressions.NullConstant);

            var newDestInstanceExp = mapExprForType[0] as BinaryExpression;
            mapExprForType.RemoveAt(0);

            var destVar = newDestInstanceExp.Left as ParameterExpression;

            var assignExistingDestExp = Expression.Assign(destVar, callSetPropMethod);

            var destCondition = Expression.IfThenElse(ifDestNull, newDestInstanceExp, assignExistingDestExp);
            mapExprForType.Insert(0, destCondition);

            var blockForSubstitution = Expression.Block(mapExprForType);
            var substBlock =
                new SubstituteParameterVisitor(sourceVariable, destVariable).Visit(blockForSubstitution) as
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

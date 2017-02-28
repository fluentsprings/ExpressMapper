using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressMapper
{
    public abstract class TypeMapperBase<T, TN> : IMemberConfigParameters
    {
        #region Constants

        private const string RightStr = "right";
        private const string DstString = "dst";
        private const string SrcString = "src";

        #endregion

        private readonly object _lockObject = new object();
        private bool _compiling;
        protected ParameterExpression DestFakeParameter = Expression.Parameter(typeof(TN), DstString);
        protected IMappingService MappingService { get; set; }
        protected IMappingServiceProvider MappingServiceProvider { get; set; }
        protected Dictionary<MemberInfo, MemberInfo> AutoMembers = new Dictionary<MemberInfo, MemberInfo>();
        public bool BaseType { get; set; }


        public IMemberConfiguration<T, TN> MemberConfiguration { get; set; }
        public List<KeyValuePair<MemberExpression, Expression>> CustomMembers { get; private set; }
        public List<KeyValuePair<MemberExpression, Expression>> FlattenMembers { get; private set; }
        public List<KeyValuePair<MemberExpression, Expression>> CustomFunctionMembers { get; private set; }
        public Expression<Func<T, TN>> QueryableExpression { get; protected set; }
        public abstract CompilationTypes MapperType { get; }
        public bool Flattened { get; set; }
        public Type SourceType => typeof(T);
        public Type DestinationType => typeof(TN);

        public Expression QueryableGeneralExpression => QueryableExpression;

        #region Constructors

        protected TypeMapperBase(IMappingService service, IMappingServiceProvider serviceProvider)
        {
            ResultExpressionList = new List<Expression>();
            RecursiveExpressionResult = new List<Expression>();
            PropertyCache = new Dictionary<string, Expression>();
            CustomPropertyCache = new Dictionary<string, Expression>();
            IgnoreMemberList = new List<string>();
            MappingService = service;
            MappingServiceProvider = serviceProvider;
            InitializeRecursiveMappings(serviceProvider);

            CustomMembers = new List<KeyValuePair<MemberExpression, Expression>>();
            FlattenMembers = new List<KeyValuePair<MemberExpression, Expression>>();
            CustomFunctionMembers = new List<KeyValuePair<MemberExpression, Expression>>();
        }

        #endregion

        #region Properties

        protected ParameterExpression SourceParameter = Expression.Parameter(typeof(T), SrcString);
        protected List<Expression> RecursiveExpressionResult { get; private set; }
        protected List<Expression> ResultExpressionList { get; private set; }
        protected Func<T, TN, TN> ResultMapFunction { get; set; }
        public List<string> IgnoreMemberList { get; private set; }
        protected Dictionary<string, Expression> PropertyCache { get; private set; }
        protected Dictionary<string, Expression> CustomPropertyCache { get; private set; }
        protected Action<T, TN> BeforeMapHandler { get; set; }
        protected Action<T, TN> AfterMapHandler { get; set; }
        protected Func<T, TN> ConstructorFunc { get; set; }
        protected Expression<Func<T, TN>> ConstructorExp { get; set; }
        protected Func<object, object, object> NonGenericMapFunc { get; set; }
        public bool CaseSensetiveMember { get; set; }
        public bool CaseSensetiveOverride { get; set; }
        public CompilationTypes CompilationTypeMember { get; set; }
        public bool CompilationTypeOverride { get; set; }

        #endregion

        protected abstract void InitializeRecursiveMappings(IMappingServiceProvider serviceProvider);

        public void Flatten()
        {
            Flattened = true;
        }

        public void CaseSensetiveMemberMap(bool caseSensitive)
        {
            CaseSensetiveMember = caseSensitive;
            CaseSensetiveOverride = true;
        }

        public void CompileTo(CompilationTypes compileType)
        {
            CompilationTypeMember = compileType;
            CompilationTypeOverride = true;
        }

        public void Compile(CompilationTypes compilationType, bool forceByDemand = false)
        {
            if (!forceByDemand && ((CompilationTypeOverride && (MapperType & CompilationTypeMember) != MapperType) ||
                                   (!CompilationTypeOverride && (MapperType & compilationType) != MapperType)))
            {
                return;
            }

            if (_compiling)
            {
                return;
            }

            try
            {
                _compiling = true;
                try
                {
                    CompileInternal();
                }
                catch (Exception ex)
                {
                    throw new ExpressmapperException(
                        $"Error error occured trying to compile mapping for: source {typeof(T).FullName}, destination {typeof(TN).FullName}. See the inner exception for details.",
                        ex);
                }
            }
            finally
            {
                _compiling = false;
            }
        }

        protected abstract void CompileInternal();

        public void AfterMap(Action<T, TN> afterMap)
        {
            if (afterMap == null)
            {
                throw new ArgumentNullException(nameof(afterMap));
            }

            if (AfterMapHandler != null)
            {
                throw new InvalidOperationException($"AfterMap already registered for {typeof(T).FullName}");
            }

            AfterMapHandler = afterMap;
        }

        public Tuple<List<Expression>, ParameterExpression, ParameterExpression> GetMapExpressions()
        {
            if (_compiling || BaseType)
            {
                return new Tuple<List<Expression>, ParameterExpression, ParameterExpression>(
                    new List<Expression>(RecursiveExpressionResult), SourceParameter, DestFakeParameter);
            }

            Compile(MapperType);
            return new Tuple<List<Expression>, ParameterExpression, ParameterExpression>(
                new List<Expression>(ResultExpressionList), SourceParameter, DestFakeParameter);
        }

        public Func<object, object, object> GetNonGenericMapFunc()
        {
            if (NonGenericMapFunc != null)
            {
                return NonGenericMapFunc;
            }

            var parameterExpression = Expression.Parameter(typeof(object), SrcString);
            var srcConverted = Expression.Convert(parameterExpression, typeof(T));
            var srcTypedExp = Expression.Variable(typeof(T), "srcTyped");
            var srcAssigned = Expression.Assign(srcTypedExp, srcConverted);

            var destParameterExp = Expression.Parameter(typeof(object), DstString);
            var dstConverted = Expression.Convert(destParameterExp, typeof(TN));
            var dstTypedExp = Expression.Variable(typeof(TN), "dstTyped");
            var dstAssigned = Expression.Assign(dstTypedExp, dstConverted);

            var customGenericType = typeof(ITypeMapper<,>).MakeGenericType(typeof(T), typeof(TN));
            var castToCustomGeneric = Expression.Convert(Expression.Constant((ITypeMapper)this), customGenericType);
            var genVariable = Expression.Variable(customGenericType);
            var assignExp = Expression.Assign(genVariable, castToCustomGeneric);
            var methodInfo = customGenericType.GetInfo().GetMethod("MapTo", new[] { typeof(T), typeof(TN) });

            var mapCall = Expression.Call(genVariable, methodInfo, srcTypedExp, dstTypedExp);
            var resultVarExp = Expression.Variable(typeof(object), "result");
            var convertToObj = Expression.Convert(mapCall, typeof(object));
            var assignResult = Expression.Assign(resultVarExp, convertToObj);

            var blockExpression = Expression.Block(new[] { srcTypedExp, dstTypedExp, genVariable, resultVarExp },
                new Expression[] { srcAssigned, dstAssigned, assignExp, assignResult, resultVarExp });
            var lambda =
                Expression.Lambda<Func<object, object, object>>(blockExpression, parameterExpression, destParameterExp);
            NonGenericMapFunc = lambda.Compile();

            return NonGenericMapFunc;
        }

        protected void AutoMapProperty(MemberInfo propertyGet, MemberInfo propertySet)
        {
            var callSetPropMethod = propertySet.MemberType == MemberTypes.Field ? Expression.Field(DestFakeParameter, propertySet as FieldInfo) : Expression.Property(DestFakeParameter, propertySet as PropertyInfo);
            var callGetPropMethod = propertyGet.MemberType == MemberTypes.Field ? Expression.Field(SourceParameter, propertyGet as FieldInfo) : Expression.Property(SourceParameter, propertyGet as PropertyInfo);

            MapMember(callSetPropMethod, callGetPropMethod);
        }

        public void MapMember<TSourceMember, TDestMember>(Expression<Func<TN, TDestMember>> left,
            Expression<Func<T, TSourceMember>> right)
        {
            if (left == null)
            {
                throw new ArgumentNullException("left");
            }

            if (right == null)
            {
                throw new ArgumentNullException(RightStr);
            }

            CustomMembers.Add(
                new KeyValuePair<MemberExpression, Expression>(left.Body as MemberExpression, right.Body));
            //MapMember(left.Body as MemberExpression, right.Body);
        }

        #region flatten code

        public void MapMemberFlattened(MemberExpression left, Expression right)
        {
            FlattenMembers.Add(new KeyValuePair<MemberExpression, Expression>(left, right));
        }

        protected List<string> NamesOfMembersAndIgnoredProperties()
        {
            var result =
                CustomMembers.Select(x => x.Key.Member.Name)
                    .Union(CustomFunctionMembers.Select(x => x.Key.Member.Name))
                    .ToList();
            result.AddRange(IgnoreMemberList);
            return result;
        }

        #endregion


        protected void MapMember(MemberExpression left, Expression right)
        {
            var mappingExpression = MappingService.GetMemberMappingExpression(left, right, false);
            CustomPropertyCache[left.Member.Name] = mappingExpression;
        }

        protected BinaryExpression GetDestionationVariable()
        {
            if (ConstructorExp != null)
            {
                var substVisitorSrc = new SubstituteParameterVisitor(SourceParameter);
                var constructorExp = substVisitorSrc.Visit(ConstructorExp.Body);

                return Expression.Assign(DestFakeParameter, constructorExp);
            }

            if (ConstructorFunc != null)
            {
                Expression<Func<T, TN>> customConstruct = t => ConstructorFunc(t);
                var invocationExpression = Expression.Invoke(customConstruct, SourceParameter);
                return Expression.Assign(DestFakeParameter, invocationExpression);
            }
            var createDestination = Expression.New(typeof(TN));
            return Expression.Assign(DestFakeParameter, createDestination);
        }

        protected void ProcessAutoProperties()
        {
            var getFields =
                typeof(T).GetInfo()
                    .GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);
            var setFields =
                typeof(TN).GetInfo()
                    .GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);

            var getProps =
                typeof(T).GetInfo()
                    .GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);

            var setProps =
                typeof(TN).GetInfo()
                    .GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);

            var sourceMembers = getFields.Cast<MemberInfo>().Union(getProps);
            var destMembers = setFields.Cast<MemberInfo>().Union(setProps);

            var stringComparison = GetStringCase();

            //var comparer = CultureInfo.CurrentCulture.CompareInfo.GetStringComparer(CompareOptions.OrdinalIgnoreCase);
            var comparer = StringComparer.Create(CultureInfo.CurrentCulture,
                stringComparison == StringComparison.OrdinalIgnoreCase);

            foreach (var prop in sourceMembers)
            {
                if (IgnoreMemberList.Contains(prop.Name, comparer) ||
                    CustomPropertyCache.Keys.Contains(prop.Name, comparer))
                {
                    continue;
                }

                var notUniqueDestMembers = destMembers.Where(x => string.Equals(x.Name, prop.Name, stringComparison));
                var notUniqueSrcMembers = sourceMembers.Where(x => string.Equals(x.Name, prop.Name, stringComparison));

                var getprop = GetTopMostMemberOfHierarchy(notUniqueSrcMembers);
                if (AutoMembers.ContainsKey(getprop))
                {
                    continue;
                }

                var setprop = GetTopMostMemberOfHierarchy(notUniqueDestMembers);

                var propertyInfo = setprop as PropertyInfo;
                if ((propertyInfo == null && setprop == null) ||
                    (propertyInfo != null && (!propertyInfo.CanWrite || !propertyInfo.GetSetMethod(true).IsPublic)))
                {
                    IgnoreMemberList.Add(getprop.Name);
                    continue;
                }
                AutoMembers[getprop] = setprop;
                AutoMapProperty(getprop, setprop);
            }
        }

        private static MemberInfo GetTopMostMemberOfHierarchy(IEnumerable<MemberInfo> notUniqueMembers)
        {
            MemberInfo chosen = null;

            foreach (var notUniqueMember in notUniqueMembers)
            {
                if (chosen == null)
                {
                    chosen = notUniqueMember;
                }
                else
                {
                    chosen = chosen.DeclaringType.GetInfo().IsAssignableFrom(notUniqueMember.DeclaringType)
                        ? notUniqueMember
                        : chosen;
                }
            }
            return chosen;
        }

        internal StringComparison GetStringCase()
        {
            StringComparison stringComparison;

            if (MappingServiceProvider.CaseSensetiveMemberMap && !CaseSensetiveOverride)
            {
                stringComparison = MappingServiceProvider.CaseSensetiveMemberMap
                    ? StringComparison.CurrentCulture
                    : StringComparison.OrdinalIgnoreCase;
            }
            else
            {
                stringComparison = CaseSensetiveMember
                    ? StringComparison.CurrentCulture
                    : StringComparison.OrdinalIgnoreCase;
            }
            return stringComparison;
        }

        public virtual void InstantiateFunc(Func<T, TN> constructor)
        {
            ConstructorFunc = constructor;
        }

        public virtual void Instantiate(Expression<Func<T, TN>> constructor)
        {
            ConstructorExp = constructor;
        }

        public virtual void BeforeMap(Action<T, TN> beforeMap)
        {
            if (beforeMap == null)
            {
                throw new ArgumentNullException(nameof(beforeMap));
            }

            if (BeforeMapHandler != null)
            {
                throw new InvalidOperationException($"BeforeMap already registered for {typeof(T).FullName}");
            }

            BeforeMapHandler = beforeMap;
        }

        public virtual void Ignore<TMember>(Expression<Func<TN, TMember>> left)
        {
            var memberExpression = left.Body as MemberExpression;
            IgnoreMemberList.Add(memberExpression.Member.Name);
        }

        public void Ignore(PropertyInfo left)
        {
            IgnoreMemberList.Add(left.Name);
        }

        public TN MapTo(T src, TN dest)
        {
            if (ResultMapFunction == null)
            {
                lock (_lockObject)
                {
                    // force compilation by client code demand
                    Compile(MapperType, forceByDemand: true);
                }
            }
            return ResultMapFunction(src, dest);
        }

        public void MapFunction<TMember, TNMember>(Expression<Func<TN, TNMember>> left, Func<T, TMember> right)
        {
            var memberExpression = left.Body as MemberExpression;
            Expression<Func<T, TMember>> expr = (t) => right(t);

            var parameterExpression = Expression.Parameter(typeof(T));
            var rightExpression = Expression.Invoke(expr, parameterExpression);

            CustomFunctionMembers.Add(
                new KeyValuePair<MemberExpression, Expression>(memberExpression, rightExpression));
            //MapFunction<TMember, TNMember>(left, rightExpression, memberExpression);
        }

        protected void MapFunction(MemberExpression left, Expression rightExpression)
        {
            if (left.Member.DeclaringType != rightExpression.Type)
            {
                var mapComplexResult =
                    MappingService.GetDifferentTypeMemberMappingExpression(rightExpression, left, false);
                CustomPropertyCache[left.Member.Name] = mapComplexResult;
            }
            else
            {
                var binaryExpression = Expression.Assign(left, rightExpression);
                CustomPropertyCache[left.Member.Name] = binaryExpression;
            }
        }

        protected void ProcessCustomMembers()
        {
            CustomMembers = TranslateExpression(CustomMembers);
            foreach (var keyValue in CustomMembers)
            {
                MapMember(keyValue.Key, keyValue.Value);
            }
        }

        protected void ProcessCustomFunctionMembers()
        {
            CustomFunctionMembers = TranslateExpression(CustomFunctionMembers);
            foreach (var keyValue in CustomFunctionMembers)
            {
                MapMember(keyValue.Key, keyValue.Value);
            }
        }

        protected void ProcessFlattenedMembers()
        {
            if (Flattened)
            {
                var flattenMapper = new FlattenMapper<T, TN>(NamesOfMembersAndIgnoredProperties(), GetStringCase());
                foreach (var flattenInfo in flattenMapper.BuildMemberMapping())
                {
                    MapMemberFlattened(flattenInfo.DestAsMemberExpression<TN>(), flattenInfo.SourceAsExpression<T>());
                }

                FlattenMembers = TranslateExpression(FlattenMembers);
                foreach (var keyValue in FlattenMembers)
                {
                    MapMember(keyValue.Key, keyValue.Value);
                }
            }
        }

        protected List<KeyValuePair<MemberExpression, Expression>> TranslateExpression(
            IEnumerable<KeyValuePair<MemberExpression, Expression>> expressions)
        {
            var result = new List<KeyValuePair<MemberExpression, Expression>>(expressions.Count());
            foreach (var customMember in expressions)
            {
                var substVisitorDest = new SubstituteParameterVisitor(DestFakeParameter);
                var dest = substVisitorDest.Visit(customMember.Key) as MemberExpression;

                var substVisitorSrc = new SubstituteParameterVisitor(SourceParameter);
                var src = substVisitorSrc.Visit(customMember.Value);
                result.Add(new KeyValuePair<MemberExpression, Expression>(dest, src));
            }
            return result;
        }

        public void ImportMemberConfigParameters(IMemberConfigParameters baseClassConfiguration)
        {
            Flattened = baseClassConfiguration.Flattened;
            CaseSensetiveMember = baseClassConfiguration.CaseSensetiveMember;
            CaseSensetiveOverride = baseClassConfiguration.CaseSensetiveOverride;
            CompilationTypeOverride = baseClassConfiguration.CompilationTypeOverride;
            CompilationTypeMember = baseClassConfiguration.CompilationTypeMember;

            // todo : implement visitor to replace base type to the subclass' type
            CustomFunctionMembers =
                new List<KeyValuePair<MemberExpression, Expression>>(baseClassConfiguration.CustomFunctionMembers
                    .Count);
            CustomMembers =
                new List<KeyValuePair<MemberExpression, Expression>>(baseClassConfiguration.CustomMembers.Count);
            FlattenMembers =
                new List<KeyValuePair<MemberExpression, Expression>>(baseClassConfiguration.FlattenMembers.Count);

            IgnoreMemberList =
                new List<string>(baseClassConfiguration.IgnoreMemberList);

            var replaceDestMemberTypeVisitor = new ReplaceMemberTypeVisitor(DestinationType, DestFakeParameter);
            var replaceSrcMemberTypeVisitor = new ReplaceMemberTypeVisitor(SourceType, SourceParameter);

            foreach (var customMember in baseClassConfiguration.CustomMembers)
            {
                var destExp = replaceDestMemberTypeVisitor.Visit(customMember.Key) as MemberExpression;
                var srcExp = replaceSrcMemberTypeVisitor.Visit(customMember.Value);
                CustomMembers.Add(new KeyValuePair<MemberExpression, Expression>(destExp, srcExp));
            }

            foreach (var customMember in baseClassConfiguration.CustomFunctionMembers)
            {
                var destExp = replaceDestMemberTypeVisitor.Visit(customMember.Key) as MemberExpression;
                var srcExp = replaceSrcMemberTypeVisitor.Visit(customMember.Value);
                CustomFunctionMembers.Add(new KeyValuePair<MemberExpression, Expression>(destExp, srcExp));
            }

            foreach (var customMember in baseClassConfiguration.FlattenMembers)
            {
                var destExp = replaceDestMemberTypeVisitor.Visit(customMember.Key) as MemberExpression;
                var srcExp = replaceSrcMemberTypeVisitor.Visit(customMember.Value);
                FlattenMembers.Add(new KeyValuePair<MemberExpression, Expression>(destExp, srcExp));
            }
        }
    }

    /// <summary>
    /// ReplaceMemberTypeVisitor
    /// </summary>
    public class ReplaceMemberTypeVisitor : ExpressionVisitor
    {
        private readonly Type _replacementType;
        private readonly Expression _instanceExp;

        /// <summary>
        /// ReplaceMemberTypeVisitor constructor
        /// </summary>
        public ReplaceMemberTypeVisitor(Type replacementType, Expression instanceExp)
        {
            _replacementType = replacementType;
            _instanceExp = instanceExp;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return node.Member.DeclaringType.GetInfo().IsAssignableFrom(_replacementType)
                ? Expression.PropertyOrField(_instanceExp, node.Member.Name)
                : base.VisitMember(node);
        }
    }
}

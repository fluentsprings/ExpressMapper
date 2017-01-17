using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressMapper
{
    public class SourceTypeMapper<T, TN> : TypeMapperBase<T, TN>, ITypeMapper<T, TN>
    {
        private readonly Dictionary<string, MemberBinding> _bindingExpressions = new Dictionary<string, MemberBinding>();

        public SourceTypeMapper(IMappingService service, IMappingServiceProvider serviceProvider) : base(service, serviceProvider) { }

        public override CompilationTypes MapperType => CompilationTypes.Source;

        protected override void InitializeRecursiveMappings(IMappingServiceProvider serviceProvider)
        {
            var mapMethod =
                typeof(IMappingServiceProvider).GetInfo().GetMethods()
                    .First(mi => mi.Name == "Map" && mi.GetParameters().Length == 1)
                    .MakeGenericMethod(typeof(T), typeof(TN));

            var methodCall = Expression.Call(Expression.Constant(serviceProvider), mapMethod, SourceParameter);

            RecursiveExpressionResult.Add(Expression.Assign(DestFakeParameter, methodCall));
        }

        protected override void CompileInternal()
        {
            if (ResultMapFunction != null) return;

            var destVariable = GetDestionationVariable();

            ProcessCustomMembers();
            ProcessCustomFunctionMembers();
            ProcessFlattenedMembers();
            ProcessAutoProperties();

            CreateQueryableProjection();

            var expressions = new List<Expression> { destVariable };

            if (BeforeMapHandler != null)
            {
                Expression<Action<T, TN>> beforeExpression = (src, dest) => BeforeMapHandler(src, dest);
                var beforeInvokeExpr = Expression.Invoke(beforeExpression, SourceParameter, destVariable.Left);
                expressions.Add(beforeInvokeExpr);
            }

            expressions.AddRange(PropertyCache.Values);

            var customProps = CustomPropertyCache.Where(k => !IgnoreMemberList.Contains(k.Key)).Select(k => k.Value);
            expressions.AddRange(customProps);

            if (AfterMapHandler != null)
            {
                Expression<Action<T, TN>> afterExpression = (src, dest) => AfterMapHandler(src, dest);
                var afterInvokeExpr = Expression.Invoke(afterExpression, SourceParameter, destVariable.Left);
                expressions.Add(afterInvokeExpr);
            }

            ResultExpressionList.AddRange(expressions);
            expressions.Add(destVariable.Left);

            var variables = new List<ParameterExpression>();

            var finalExpression = Expression.Block(variables, expressions);

            var destExpression = destVariable.Left as ParameterExpression;

            var substituteParameterVisitor =
                new PreciseSubstituteParameterVisitor(
                    new KeyValuePair<ParameterExpression, ParameterExpression>(SourceParameter, SourceParameter),
                    new KeyValuePair<ParameterExpression, ParameterExpression>(destExpression, destExpression));

            var resultExpression = substituteParameterVisitor.Visit(finalExpression) as BlockExpression;

            var expression = Expression.Lambda<Func<T, TN, TN>>(resultExpression, SourceParameter, DestFakeParameter);
            ResultMapFunction = expression.Compile();
        }

        private void CreateQueryableProjection()
        {
            try
            {
                foreach (var customMember in CustomMembers)
                {
                    ProcessProjectingMember(customMember.Value, customMember.Key.Member as PropertyInfo);
                }

                foreach (var flattenedMember in FlattenMembers)
                {
                    ProcessProjectingMember(flattenedMember.Value, flattenedMember.Key.Member as PropertyInfo);
                }

                foreach (var autoMember in AutoMembers)
                {

                    if (_bindingExpressions.ContainsKey(autoMember.Value.Name) || IgnoreMemberList.Contains(autoMember.Value.Name)) continue;

                    var destination = autoMember.Value as PropertyInfo;
                    var propertyOrField = Expression.PropertyOrField(SourceParameter, autoMember.Key.Name);
                    ProcessProjectingMember(propertyOrField, destination);
                }

                QueryableExpression =
                    Expression.Lambda<Func<T, TN>>(
                        Expression.MemberInit(Expression.New(typeof(TN)), _bindingExpressions.Values), SourceParameter);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Queryable projection is not supported for such mapping. Exception: {0}", ex);
            }
        }

        private void ProcessProjectingMember(Expression sourceExp, PropertyInfo destProp)
        {
            var memberQueryableExpression =
                MappingService.GetMemberQueryableExpression(sourceExp.Type,
                    destProp.PropertyType);

            var tCol =
                sourceExp.Type.GetInfo().GetInterfaces()
                    .FirstOrDefault(t => t.GetInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                (sourceExp.Type.GetInfo().IsGenericType
                 && sourceExp.Type.GetInfo().GetInterfaces().Any(t => t == typeof(IEnumerable))
                    ? sourceExp.Type
                    : null);

            var tnCol = destProp.PropertyType.GetInfo().GetInterfaces()
                            .FirstOrDefault(t => t.GetInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                        (destProp.PropertyType.GetInfo().IsGenericType &&
                         destProp.PropertyType.GetInfo().GetInterfaces().Any(t => t == typeof(IEnumerable))
                            ? destProp.PropertyType
                            : null);

            if (sourceExp.Type != typeof(string) && tCol != null && tnCol != null)
            {
                var sourceGenericType = sourceExp.Type.GetInfo().GetGenericArguments()[0];
                var destGenericType = destProp.PropertyType.IsArray ? destProp.PropertyType.GetElementType() : destProp.PropertyType.GetInfo().GetGenericArguments()[0];

                var genericMemberQueryableExpression =
                    MappingService.GetMemberQueryableExpression(sourceGenericType,
                        destGenericType);


                MethodInfo selectMethod = null;
                foreach (
                    var p in from m in typeof(Enumerable).GetInfo().GetMethods().Where(m => m.Name == "Select")
                    from p in m.GetParameters().Where(p => p.Name.Equals("selector"))
                    where p.ParameterType.GetInfo().GetGenericArguments().Count() == 2
                    select p)
                    selectMethod = (MethodInfo)p.Member;

                Expression selectExpression = Expression.Call(
                    null,
                    selectMethod.MakeGenericMethod(sourceGenericType, destGenericType),
                    new[] { sourceExp, genericMemberQueryableExpression });

                var destListAndCollTest = typeof(ICollection<>).MakeGenericType(destGenericType).GetInfo().IsAssignableFrom(destProp.PropertyType);

                if (destListAndCollTest)
                {
                    var toArrayMethod = typeof(Enumerable).GetInfo().GetMethod("ToList");
                    selectExpression = Expression.Call(null, toArrayMethod.MakeGenericMethod(destGenericType), selectExpression);
                }

                _bindingExpressions.Add(destProp.Name,
                    Expression.Bind(destProp, selectExpression));
            }
            else
            {
                Expression expression;
                if (memberQueryableExpression != null)
                {
                    var lambdaExpression = memberQueryableExpression as LambdaExpression;
                    var projectionAccessMemberVisitor = new ProjectionAccessMemberVisitor(sourceExp.Type, sourceExp);
                    var clearanceExp = projectionAccessMemberVisitor.Visit(lambdaExpression.Body);
                    expression =
                        Expression.Condition(
                            Expression.Equal(sourceExp, Expression.Constant(null, sourceExp.Type)),
                            Expression.Constant(null, destProp.PropertyType), clearanceExp);
                }
                else
                {
                    expression = sourceExp;
                }
                _bindingExpressions.Add(destProp.Name,
                    Expression.Bind(destProp, expression));
            }
        }
    }
}

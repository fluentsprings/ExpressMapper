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

        public SourceTypeMapper(IMappingService service) : base(service){}

        protected override void InitializeRecursiveMappings()
        {
            var mapMethod =
                typeof (Mapper).GetMethods()
                    .First(mi => mi.Name == "Map" && mi.GetParameters().Length == 1)
                    .MakeGenericMethod(typeof (T), typeof (TN));
            
            RecursiveExpressionResult.Add(Expression.Assign(DestFakeParameter, Expression.Call(mapMethod, SourceParameter)));
        }

        protected override void CompileInternal()
        {
            if (ResultMapFunction != null) return;

            var destVariable = GetDestionationVariable();

            ProcessCustomMembers();
            ProcessCustomFunctionMembers();
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

            //var substituteParameterVisitor = new SubstituteParameterVisitor(SourceParameter,
            //    destVariable.Left as ParameterExpression);

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
                    var memberQueryableExpression =
                        MappingService.GetMemberQueryableExpression(customMember.Key.Member.DeclaringType,
                            customMember.Value.Type);
                    var expression = memberQueryableExpression ?? customMember.Value;
                    _bindingExpressions.Add(customMember.Key.Member.Name,
                        Expression.Bind(customMember.Key.Member, expression));
                }

                foreach (var customMember in CustomFunctionMembers)
                {
                    if (_bindingExpressions.ContainsKey(customMember.Key.Member.Name)) continue;

                    var memberQueryableExpression =
                        MappingService.GetMemberQueryableExpression(customMember.Key.Member.DeclaringType,
                            customMember.Value.Type);
                    var expression = memberQueryableExpression ?? customMember.Value;
                    _bindingExpressions.Add(customMember.Key.Member.Name,
                        Expression.Bind(customMember.Key.Member, expression));
                }

                foreach (var autoMember in AutoMembers)
                {
                    if (_bindingExpressions.ContainsKey(autoMember.Value.Name)) continue;

                    var source = autoMember.Key as PropertyInfo;
                    var destination = autoMember.Value as PropertyInfo;

                    var memberQueryableExpression =
                        MappingService.GetMemberQueryableExpression(source.PropertyType,
                            destination.PropertyType);

                    var propertyOrField = Expression.PropertyOrField(SourceParameter, autoMember.Key.Name);

                    var tCol =
                    source.PropertyType.GetInterfaces()
                    .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                    (source.PropertyType.IsGenericType
                        && source.PropertyType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? source.PropertyType
                        : null);

                    var tnCol = destination.PropertyType.GetInterfaces()
                        .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ??
                                 (destination.PropertyType.IsGenericType && destination.PropertyType.GetInterfaces().Any(t => t == typeof(IEnumerable)) ? destination.PropertyType
                                     : null);

                    if (source.PropertyType != typeof(string) && tCol != null && tnCol != null)
                    {
                        var sourceGenericType = source.PropertyType.GetGenericArguments()[0];
                        var destGenericType = destination.PropertyType.GetGenericArguments()[0];

                        var genericMemberQueryableExpression =
                        MappingService.GetMemberQueryableExpression(sourceGenericType,
                            destGenericType);

                        MethodInfo selectMethod = null;
                        foreach (MethodInfo m in typeof(Enumerable).GetMethods().Where(m => m.Name == "Select"))
                            foreach (ParameterInfo p in m.GetParameters().Where(p => p.Name.Equals("selector")))
                                if (p.ParameterType.GetGenericArguments().Count() == 2)
                                    selectMethod = (MethodInfo)p.Member;

                        var selectExpression = Expression.Call(
                          null,
                          selectMethod.MakeGenericMethod(new Type[] { sourceGenericType, destGenericType }),
                          new Expression[] { propertyOrField, genericMemberQueryableExpression });

                        _bindingExpressions.Add(autoMember.Value.Name,
                        Expression.Bind(autoMember.Value, selectExpression));
                    }
                    else
                    {
                        Expression expression;
                        if (memberQueryableExpression != null)
                        {
                            var lambdaExpression = memberQueryableExpression as LambdaExpression;
                            var projectionAccessMemberVisitor = new ProjectionAccessMemberVisitor(propertyOrField.Type, propertyOrField);
                            var clearanceExp = projectionAccessMemberVisitor.Visit(lambdaExpression.Body);
                            expression =
                                Expression.Condition(
                                    Expression.Equal(propertyOrField, Expression.Constant(null, propertyOrField.Type)),
                                    Expression.Constant(null, ((PropertyInfo)autoMember.Value).PropertyType), clearanceExp);
                        }
                        else
                        {
                            expression = propertyOrField;
                        }
                        _bindingExpressions.Add(autoMember.Value.Name,
                            Expression.Bind(autoMember.Value, expression));
                    }
                }

                QueryableExpression =
                    Expression.Lambda<Func<T, TN>>(
                        Expression.MemberInit(Expression.New(typeof (TN)), _bindingExpressions.Values), SourceParameter);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Queryable projection is not supported for such mapping. Exception: {0}", ex);
            }
        }
    }
}

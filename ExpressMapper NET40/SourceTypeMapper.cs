using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressMapper
{
    public class SourceTypeMapper<T, TN> : TypeMapperBase<T, TN>, ITypeMapper<T, TN>
    {
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
    }
}

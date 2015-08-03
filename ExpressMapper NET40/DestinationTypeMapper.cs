using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressMapper
{
    public class DestinationTypeMapper<T, TN> : TypeMapperBase<T, TN>, ITypeMapper<T, TN>
    {
        public DestinationTypeMapper(IMappingService service) : base(service){}
        public override void Compile()
        {
            if (ResultMapFunction != null) return;

            ProcessAutoProperties();

            var expressions = new List<Expression>();

            if (BeforeMapHandler != null)
            {
                Expression<Action<T, TN>> beforeExpression = (src, dest) => BeforeMapHandler(src, dest);
                var beforeInvokeExpr = Expression.Invoke(beforeExpression, SourceParameter, DestFakeParameter);
                expressions.Add(beforeInvokeExpr);
            }

            expressions.AddRange(PropertyCache.Values);

            var customProps = CustomPropertyCache.Where(k => !IgnoreMemberList.Contains(k.Key)).Select(k => k.Value);
            expressions.AddRange(customProps);

            if (AfterMapHandler != null)
            {
                Expression<Action<T, TN>> afterExpression = (src, dest) => AfterMapHandler(src, dest);
                var afterInvokeExpr = Expression.Invoke(afterExpression, SourceParameter, DestFakeParameter);
                expressions.Add(afterInvokeExpr);
            }

            ResultExpressionList.AddRange(expressions);
            ResultExpressionList.Insert(0, GetDestionationVariable());

            expressions.Add(DestFakeParameter);

            var finalExpression = Expression.Block(expressions);
            var substituteParameterVisitor = new SubstituteParameterVisitor(SourceParameter, DestFakeParameter);
            var resultExpression = substituteParameterVisitor.Visit(finalExpression);

            var expression = Expression.Lambda<Func<T, TN, TN>>(resultExpression, SourceParameter, DestFakeParameter);
            ResultMapFunction = expression.Compile();
        }
    }
}

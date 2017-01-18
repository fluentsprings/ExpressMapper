using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressMapper
{
    public class DestinationTypeMapper<T, TN> : TypeMapperBase<T, TN>, ITypeMapper<T, TN>
    {
        #region Const

        private const string MapStr = "Map";

        #endregion

        public DestinationTypeMapper(IMappingService service, IMappingServiceProvider serviceProvider) : base(service, serviceProvider) { }

        public override CompilationTypes MapperType => CompilationTypes.Destination;

        protected override void InitializeRecursiveMappings(IMappingServiceProvider serviceProvider)
        {
            var mapMethod =
                typeof(IMappingServiceProvider).GetInfo().GetMethods()
                    .First(mi => mi.Name == MapStr && mi.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), typeof(TN));

            var methodCall = Expression.Call(Expression.Constant(serviceProvider), mapMethod, SourceParameter, DestFakeParameter);

            RecursiveExpressionResult.Add(Expression.Assign(DestFakeParameter, methodCall));
        }

        protected override void CompileInternal()
        {
            if (ResultMapFunction != null) return;

            ProcessCustomMembers();
            ProcessCustomFunctionMembers();
            ProcessFlattenedMembers();
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

            var substituteParameterVisitor =
                new PreciseSubstituteParameterVisitor(
                    new KeyValuePair<ParameterExpression, ParameterExpression>(SourceParameter, SourceParameter),
                    new KeyValuePair<ParameterExpression, ParameterExpression>(DestFakeParameter, DestFakeParameter));

            //var substituteParameterVisitor = new SubstituteParameterVisitor(SourceParameter, DestFakeParameter);

            var resultExpression = substituteParameterVisitor.Visit(finalExpression);

            var expression = Expression.Lambda<Func<T, TN, TN>>(resultExpression, SourceParameter, DestFakeParameter);
            ResultMapFunction = expression.Compile();
        }
    }
}

using System.Linq.Expressions;

namespace ExpressMapper
{
    internal static class StaticExpressions
    {
        internal static readonly ConstantExpression NullConstant = Expression.Constant(null);
        internal static readonly ConstantExpression FalseConstant = Expression.Constant(false);
    }
}

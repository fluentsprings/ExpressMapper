using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpressMapper
{
    public class PreciseSubstituteParameterVisitor : ExpressionVisitor
    {
        private KeyValuePair<ParameterExpression, ParameterExpression> _src;
        private KeyValuePair<ParameterExpression, ParameterExpression> _dest;

        public PreciseSubstituteParameterVisitor(KeyValuePair<ParameterExpression, ParameterExpression> src, KeyValuePair<ParameterExpression, ParameterExpression> dest)
        {
            _src = src;
            _dest = dest;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return ((_src.Key.Name == node.Name && _src.Key.Type == node.Type) ? _src.Value : (_dest.Key.Name == node.Name && _dest.Key.Type == node.Type) ? _dest.Value : base.VisitParameter(node));
        }
    }
}

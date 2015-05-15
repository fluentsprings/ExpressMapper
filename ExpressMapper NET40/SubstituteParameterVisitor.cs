using System.Linq;
using System.Linq.Expressions;

namespace ExpressMapper
{
    public class SubstituteParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression[] _parametersToReplace;

        public SubstituteParameterVisitor(params ParameterExpression[] parametersToReplace)
        {
            _parametersToReplace = parametersToReplace;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            var substitution = _parametersToReplace.FirstOrDefault(p => p.Type == node.Type);
            return substitution ?? base.VisitParameter(node);
        }
    }
}

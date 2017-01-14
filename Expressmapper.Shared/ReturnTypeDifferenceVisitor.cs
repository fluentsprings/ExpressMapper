using System.Linq.Expressions;

namespace ExpressMapper
{
    public class ReturnTypeDifferenceVisitor : ExpressionVisitor
    {
        private readonly Expression _srcExp;
        public bool DifferentReturnTypes { get; set; }

        public ReturnTypeDifferenceVisitor(Expression srcExp)
        {
            _srcExp = srcExp;
        }


        protected override Expression VisitMember(MemberExpression node)
        {
            var memberExpression = _srcExp as MemberExpression;
            if (memberExpression == null)
            {
                DifferentReturnTypes = true;
                return base.VisitMember(node);
            }
            DifferentReturnTypes = memberExpression != node;
            return base.VisitMember(node);
        }
    }
}

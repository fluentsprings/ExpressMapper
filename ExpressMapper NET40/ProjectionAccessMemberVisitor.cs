using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace ExpressMapper
{
    public class ProjectionAccessMemberVisitor : ExpressionVisitor
    {
        private Expression _exp;
        //private ParameterExpression _src;
        private Type _type;
        public ProjectionAccessMemberVisitor(Type type, Expression exp)
        {
            _exp = exp;
            _type = type;
            //_src = src;
        }

        //protected override Expression VisitParameter(ParameterExpression node)
        //{
        //    if (node.Type == _type)
        //    {
        //        return _src;
        //    }
        //    return base.VisitParameter(node);
        //}

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression.Type == _type)
            {
                return Expression.PropertyOrField(_exp, node.Member.Name);
            }
            return base.VisitMember(node);
        }
    }
}

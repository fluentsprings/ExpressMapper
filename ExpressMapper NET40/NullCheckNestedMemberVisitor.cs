using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpressMapper
{
    public class NullCheckNestedMemberVisitor : ExpressionVisitor
    {
        private readonly List<string> _uniquefList = new List<string>();
        public Expression CheckNullExpression { get; set; }

        protected override Expression VisitMember(MemberExpression node)
        {
            var memberExpression = node.Expression as MemberExpression;

            if (memberExpression != null)
            {
                if (!_uniquefList.Contains(memberExpression.ToString()))
                {
                    _uniquefList.Add(memberExpression.ToString());
                    if (CheckNullExpression == null)
                    {
                        CheckNullExpression = Expression.Equal(memberExpression, Expression.Default(memberExpression.Type));
                    }
                    else
                    {
                        CheckNullExpression =
                            Expression.OrElse(Expression.Equal(memberExpression, Expression.Default(memberExpression.Type)),
                                CheckNullExpression);
                    }
                }
            }

            return base.VisitMember(node);
        }
    }
}

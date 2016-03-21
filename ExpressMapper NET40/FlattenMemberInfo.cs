using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ExpressMapper
{
    internal class FlattenMemberInfo
    {
        public FlattenMemberInfo(PropertyInfo destMember, PropertyInfo[] sourcePathMembers, PropertyInfo lastMemberToAdd, 
            FlattenLinqMethod linqMethodSuffix  = null)
        {
            DestMember = destMember;
            LinqMethodSuffix = linqMethodSuffix;

            var list = sourcePathMembers.ToList();
            list.Add(lastMemberToAdd);
            SourcePathMembers = list;
        }

        /// <summary>
        /// The Destination property in the DTO
        /// </summary>
        public PropertyInfo DestMember { get; private set; }

        /// <summary>
        /// The list of properties in order to get to the source property we want
        /// </summary>
        public ICollection<PropertyInfo> SourcePathMembers { get; private set; }

        /// <summary>
        /// Optional Linq Method to apply to an enumerable source
        /// </summary>
        public FlattenLinqMethod LinqMethodSuffix { get; private set; }

        public override string ToString()
        {
            var linqMethodStr = LinqMethodSuffix?.ToString() ?? "";
            return $"dest => dest.{DestMember.Name}, src => src.{string.Join(".",SourcePathMembers.Select(x => x.Name))}{linqMethodStr}";
        }

        public MemberExpression  DestAsMemberExpression<TDest>()
        {
            return Expression.Property(Expression.Parameter(typeof(TDest), "dest"), DestMember);
        }

        public Expression SourceAsExpression<TSource>()
        {
            var paramExpression = Expression.Parameter(typeof(TSource), "src");
            return NestedExpressionProperty(paramExpression, SourcePathMembers.Reverse().ToArray());
        }

        //-------------------------------------------------------
        //private methods

        private Expression NestedExpressionProperty(Expression expression, PropertyInfo [] properties)
        {
            if (properties.Length > 1)
            {
                return Expression.Property(
                    NestedExpressionProperty(
                        expression,
                        properties.Skip(1).ToArray()
                        ),
                    properties[0]);
            }

            //we are at the end 
            var finalProperty = Expression.Property(expression, properties[0]);

            return LinqMethodSuffix == null
                ? (Expression)finalProperty
                : LinqMethodSuffix.AsMethodCallExpression(finalProperty, properties[0], DestMember);
        }

    }
}
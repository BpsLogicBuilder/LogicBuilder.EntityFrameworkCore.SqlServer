using LogicBuilder.Expressions.Utils;
using LogicBuilder.Expressions.Utils.Expansions;
using System;
using System.Linq.Expressions;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.Visitors
{
    internal class QueryFunctionAppender(Expression expression, ExpansionOptions expansion) : ExpressionVisitor
    {
        private readonly ExpansionOptions expansion = expansion;
        private readonly Expression expression = expression;

        public static Expression AppendQueryMethod(Expression expression, ExpansionOptions expansion)
            => new QueryFunctionAppender(expression, expansion).Visit(expression);

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "Select"//both expansion.MemberType and node.Type will be lists
                && expansion.MemberType.GetUnderlyingElementType() == node.Type.GetUnderlyingElementType()
                && this.expression.ToString().StartsWith(node.ToString()))//makes sure we're not updating some nested "Select"
            {
                return node.GetOrderBy(node.GetUnderlyingElementType(), expansion.QueryOption!.SortCollection);//QueryFunctionAppender.AppendQueryMethod is called from QueryFunctionUpdateer.GetBindingExpression only when expansion.QueryOption != null.
            }

            return base.VisitMethodCall(node);
        }
    }
}

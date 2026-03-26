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
                if (expansion.QueryOption == null)//QueryFunctionAppender.AppendQueryMethod is called from QueryFunctionUpdateer.GetBindingExpression only when expansion.QueryOption != null.
                    throw new InvalidOperationException("QueryOption must be set to append a query method.");

                return node.GetOrderBy(node.GetUnderlyingElementType(), expansion.QueryOption.SortCollection);
            }

            return base.VisitMethodCall(node);
        }
    }
}

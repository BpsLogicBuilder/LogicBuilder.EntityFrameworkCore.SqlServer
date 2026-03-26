using LogicBuilder.Expressions.Utils;
using LogicBuilder.Expressions.Utils.Expansions;
using System;
using System.Linq.Expressions;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.Visitors
{
    internal class FilterAppender(Expression expression, ExpansionOptions expansion) : ExpressionVisitor
    {
        private readonly ExpansionOptions expansion = expansion;
        private readonly Expression expression = expression;

        public static Expression AppendFilter(Expression expression, ExpansionOptions expansion)
            => new FilterAppender(expression, expansion).Visit(expression);

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == "Select"
                && expansion.MemberType.GetUnderlyingElementType() == node.GetUnderlyingElementType()
                && this.expression.ToString().StartsWith(node.ToString()))//makes sure we're not updating some nested "Select"
            {
                if (expansion.FilterOption == null)//FilterAppender.AppendFilter is called from FilterUpdateer.GetBindingExpression only when expansion.FilterOption != null.
                    throw new InvalidOperationException("FilterOption must be set to append a filter");

                return node.GetWhereCall(expansion.FilterOption.FilterLambdaOperator.Build());
            }

            return base.VisitMethodCall(node);
        }
    }
}

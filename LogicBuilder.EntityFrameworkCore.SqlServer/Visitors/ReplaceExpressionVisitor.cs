using System;
using System.Linq.Expressions;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.Visitors
{
    internal class ReplaceExpressionVisitor(Expression source, Expression target) : ExpressionVisitor
    {
        private readonly Expression _source = source;
        private readonly Expression _target = target;

        public override Expression Visit(Expression? node)
        {
            ArgumentNullException.ThrowIfNull(node);

            if (_source == node)
                return _target;

            return base.Visit(node);
        }
    }
}

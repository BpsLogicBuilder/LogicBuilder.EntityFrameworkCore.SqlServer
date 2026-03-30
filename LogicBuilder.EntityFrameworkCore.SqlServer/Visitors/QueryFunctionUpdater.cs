using LogicBuilder.Expressions.Utils.Expansions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.Visitors
{
    internal class QueryFunctionUpdater(List<ExpansionOptions> expansions) : ChildCollectionVisitor(expansions)
    {
        public static Expression UpdaterExpansion(Expression expression, List<ExpansionOptions> expansions)
                => new QueryFunctionUpdater(expansions).Visit(expression);

        protected override Expression GetBindingExpression(MemberAssignment binding, ExpansionOptions expansion)
        {
            if (expansion.QueryOption != null)
            {
                return QueryFunctionAppender.AppendQueryMethod(binding.Expression, expansion);
            }
            else if (expansions.Count > 1)  //Mutually exclusive with expansion.QueryOption != null.                            
            {                               //There can be only one set of QueryOptions in the list.
                return UpdaterExpansion
                (
                    binding.Expression,
                    [.. expansions.Skip(1)]
                );
            }
            else
                throw new InvalidOperationException("The last expansion in the list must have a query method.");
        }
    }
}

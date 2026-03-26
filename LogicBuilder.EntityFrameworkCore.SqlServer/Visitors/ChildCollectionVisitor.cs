using LogicBuilder.Expressions.Utils;
using LogicBuilder.Expressions.Utils.Expansions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.Visitors
{
    internal abstract class ChildCollectionVisitor(List<ExpansionOptions> expansions) : ExpressionVisitor
    {
        protected readonly List<ExpansionOptions> expansions = expansions;
        private readonly List<Expression> foundExpansions = [];

        protected override Expression VisitMemberInit(MemberInitExpression node)
        {
            ExpansionOptions expansion = expansions[0];

            if (node.NewExpression.Type != expansion.ParentType)
                return base.VisitMemberInit(node);

            return Expression.MemberInit
            (
                Expression.New(node.Type),
                node.Bindings.OfType<MemberAssignment>().Aggregate
                (
                    new List<MemberBinding>(),
                    AddBinding
                )
            );

            List<MemberBinding> AddBinding(List<MemberBinding> list, MemberAssignment binding)
            {
                if (ListTypesAreEquivalent(binding.Member.GetMemberType(), expansion.MemberType)
                        && string.Compare(binding.Member.Name, expansion.MemberName, true) == 0)//found the expansion
                {
                    if (foundExpansions.Count > 0)
                        throw new NotSupportedException("Recursive queries not supported");

                    AddBindingExpression(GetBindingExpression(binding, expansion));
                }
                else
                {
                    list.Add(binding);
                }

                return list;

                void AddBindingExpression(Expression bindingExpression)
                {
                    list.Add(Expression.Bind(binding.Member, bindingExpression));
                    foundExpansions.Add(bindingExpression);
                }
            }
        }

        protected abstract Expression GetBindingExpression(MemberAssignment binding, ExpansionOptions expansion);

        protected static bool ListTypesAreEquivalent(Type bindingType, Type expansionType)
        {
            if (!bindingType.IsList() || !expansionType.IsList())
                return false;

            return bindingType.GetUnderlyingElementType() == expansionType.GetUnderlyingElementType();
        }
    }
}

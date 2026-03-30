using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using AutoMapper.Internal;
using LogicBuilder.Expressions.Utils;
using LogicBuilder.Expressions.Utils.Expansions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.Visitors
{
    internal class FilterAppender(Expression expression, ExpansionOptions expansion, IMapper mapper) : ExpressionVisitor
    {
        private readonly ExpansionOptions expansion = expansion;
        private readonly Expression expression = expression;
        private readonly IMapper mapper = mapper;

        public static Expression AppendFilter(Expression expression, ExpansionOptions expansion, IMapper mapper)
            => new FilterAppender(expression, expansion, mapper).Visit(expression);

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Type elementType = expansion.MemberType.GetUnderlyingElementType();
            if (node.Method.Name == "Select"
                && elementType == node.GetUnderlyingElementType()
                && this.expression.ToString().StartsWith(node.ToString()))//makes sure we're not updating some nested "Select"
            {
                Type parentUnderlyingType = node.Arguments[0].Type.GetUnderlyingElementType();
                Type nodeUnderlyingType = elementType;
                LambdaExpression modelFilterExpression = (LambdaExpression)expansion.FilterOption!.FilterLambdaOperator.Build();//FilterAppender.AppendFilter is called from FilterUpdateer.GetBindingExpression only when expansion.FilterOption != null.
                LambdaExpression? dataFilter = GetDateFilterExpression();
                if (dataFilter == null)
                    return node.GetWhereCall(modelFilterExpression);

                var replacedParent = GetNewParentExpression(node, dataFilter);
                var listOfArgumentsForNewMethod = GetArgumentsForNewMethod(node, replacedParent);

                return Expression.Call
                (
                    node.Method.DeclaringType!,
                    node.Method.Name,
                    node.Method.GetGenericArguments(),
                    listOfArgumentsForNewMethod
                );

                LambdaExpression? GetDateFilterExpression()
                {
                    if (parentUnderlyingType != nodeUnderlyingType)
                    {
                        var typeMap = mapper.ConfigurationProvider.Internal().ResolveTypeMap(sourceType: parentUnderlyingType, destinationType: nodeUnderlyingType);
                        if (typeMap != null)
                        {
                            Type sourceType = typeof(Func<,>).MakeGenericType(nodeUnderlyingType, typeof(bool));
                            Type destType = typeof(Func<,>).MakeGenericType(parentUnderlyingType, typeof(bool));
                            Type sourceExpressionype = typeof(Expression<>).MakeGenericType(sourceType);
                            Type destExpressionType = typeof(Expression<>).MakeGenericType(destType);
                            return mapper.MapExpression(modelFilterExpression, sourceExpressionype, destExpressionType);
                        }
                    }

                    return null;
                }

                static Expression GetNewParentExpression(MethodCallExpression node, LambdaExpression filter)
                {
                    return new ReplaceExpressionVisitor
                    (
                        node.Arguments[0],
                        node.Arguments[0].GetWhereCall(filter)
                    ).Visit(node.Arguments[0]);
                }

                static Expression[] GetArgumentsForNewMethod(MethodCallExpression node, Expression replacedParent)
                {
                    return
                    [
                        .. node.Arguments.Aggregate(new List<Expression>(), (lst, next) =>
                        {
                            if (next == node.Arguments[0])
                                lst.Add(replacedParent);
                            else
                                lst.Add(next);
                            return lst;
                        })
                    ];
                }
            }


            return base.VisitMethodCall(node);
        }
    }
}

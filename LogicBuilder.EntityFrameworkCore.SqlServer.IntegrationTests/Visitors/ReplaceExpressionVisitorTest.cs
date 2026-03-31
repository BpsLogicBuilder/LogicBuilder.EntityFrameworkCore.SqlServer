using LogicBuilder.EntityFrameworkCore.SqlServer.Visitors;
using System;
using System.Linq.Expressions;
using Xunit;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Visitors
{
    public class ReplaceExpressionVisitorTest
    {
        [Fact]
        public void Visit_ReplacesSourceExpressionWithTargetExpression()
        {
            // Arrange
            Expression<Func<int, int>> source = x => x + 1;
            Expression<Func<int, int>> target = x => x * 2;
            var visitor = new ReplaceExpressionVisitor(source.Body, target.Body);

            // Act
            var result = visitor.Visit(source.Body);

            // Assert
            Assert.Equal(target.Body.ToString(), result.ToString());
        }

        [Fact]
        public void Visit_DoesNotReplaceSourceExpressionWithTargetExpressionWhenParameterIsNotSource()
        {
            // Arrange
            Expression<Func<int, int>> source = x => x + 1;
            Expression<Func<int, int>> target = x => x * 2;
            var visitor = new ReplaceExpressionVisitor(source.Body, target.Body);
            ConstantExpression other = Expression.Constant("");

            // Act
            var result = visitor.Visit(other);

            // Assert
            Assert.Equal(other.ToString(), result.ToString());
        }

        [Fact]
        public void Visit_ThrowsIfArgumentToReplaceIsNull()
        {
            // Arrange
            Expression<Func<int, int>> source = x => x + 1;
            Expression<Func<int, int>> target = x => x * 2;
            var visitor = new ReplaceExpressionVisitor(source.Body, target.Body);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => visitor.Visit((Expression)null));
        }
    }
}

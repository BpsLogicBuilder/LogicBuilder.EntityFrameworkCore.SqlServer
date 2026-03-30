using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using LogicBuilder.EntityFrameworkCore.SqlServer.Mapping;
using LogicBuilder.EntityFrameworkCore.SqlServer.Tests.Data;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Lambda;
using LogicBuilder.Expressions.Utils.ExpressionDescriptors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.Tests
{
    public class FilterDescriptorTests
    {
        static FilterDescriptorTests()
        {
            InitializeMapperConfiguration();
        }

        public FilterDescriptorTests()
        {
            Initialize();
        }

        #region Fields
        private IServiceProvider serviceProvider;
        private static readonly string parameterName = "$it";
        #endregion Fields

        #region Inequalities
        [Theory]
        [InlineData(null, true)]
        [InlineData("", false)]
        [InlineData("Doritos", false)]
        public void EqualityOperatorWithNull(string productName, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName == null)");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(null)
                    )
                );
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("Doritos", true)]
        public void EqualityOperator(string productName, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName == \"Doritos\")");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor("Doritos", typeof(string).AssemblyQualifiedName)
                    )
                );
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("Doritos", false)]
        public void NotEqualDescriptor(string productName, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName != \"Doritos\")");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor("Doritos", typeof(string).AssemblyQualifiedName)
                    )
                );
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(5.01, true)]
        [InlineData(4.99, false)]
        public void GreaterThanDescriptor(object unitPrice, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice > Convert({0:0.00}))", 5.0));
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(5.00m, typeof(decimal).AssemblyQualifiedName)
                    )
                );
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(5.0, true)]
        [InlineData(4.99, false)]
        public void GreaterThanEqualDescriptor(object unitPrice, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice >= Convert({0:0.00}))", 5.0));
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new GreaterThanOrEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(5.00m, typeof(decimal).AssemblyQualifiedName)
                    )
                );
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(4.99, true)]
        [InlineData(5.01, false)]
        public void LessThanDescriptor(object unitPrice, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice < Convert({0:0.00}))", 5.0));
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new LessThanBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(5.00m, typeof(decimal).AssemblyQualifiedName)
                    )
                );
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData(5.0, true)]
        [InlineData(5.01, false)]
        public void LessThanOrEqualDescriptor(object unitPrice, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice <= Convert({0:0.00}))", 5.0));
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new LessThanOrEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(5.00m, typeof(decimal).AssemblyQualifiedName)
                    )
                );
        }

        [Fact]
        public void NegativeNumbers()
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(44m) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice <= Convert({0:0.00}))", -5.0));
            Assert.False(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new LessThanOrEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(-5.00m, typeof(decimal).AssemblyQualifiedName)
                    )
                );
        }

        public class DateTimeOffsetInequalitiesTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<DateTimeOffsetInequalitiesTheoryData> DateTimeOffsetInequalities_Data
            =>
            [
                new DateTimeOffsetInequalitiesTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("DateTimeOffsetProp", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("DateTimeOffsetProp", new ParameterDescriptor(parameterName))
                    ),
                    "$it => ($it.DateTimeOffsetProp == $it.DateTimeOffsetProp)"
                ),
                new DateTimeOffsetInequalitiesTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("DateTimeOffsetProp", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("DateTimeOffsetProp", new ParameterDescriptor(parameterName))
                    ),
                    "$it => ($it.DateTimeOffsetProp != $it.DateTimeOffsetProp)"
                ),
                new DateTimeOffsetInequalitiesTheoryData
                (
                    new GreaterThanOrEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("DateTimeOffsetProp", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("DateTimeOffsetProp", new ParameterDescriptor(parameterName))
                    ),
                    "$it => ($it.DateTimeOffsetProp >= $it.DateTimeOffsetProp)"
                ),
                new DateTimeOffsetInequalitiesTheoryData
                (
                    new LessThanOrEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("DateTimeOffsetProp", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("DateTimeOffsetProp", new ParameterDescriptor(parameterName))
                    ),
                    "$it => ($it.DateTimeOffsetProp <= $it.DateTimeOffsetProp)"
                )
            ];

        [Theory]
        [MemberData(nameof(DateTimeOffsetInequalities_Data), MemberType = typeof(FilterDescriptorTests))]
        public void DateTimeOffsetInequalities(DateTimeOffsetInequalitiesTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<DataTypes>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
            {
                return GetFilter<T>
                (
                    theoryData.FilterBody
                );
            }
        }

        public class DateInEqualitiesTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<DateInEqualitiesTheoryData> DateInEqualities_Data
            =>
            [
                new DateInEqualitiesTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("DateTimeProp", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("DateTimeProp", new ParameterDescriptor(parameterName))
                    ),
                    "$it => ($it.DateTimeProp == $it.DateTimeProp)"
                ),
                new DateInEqualitiesTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("DateTimeProp", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("DateTimeProp", new ParameterDescriptor(parameterName))
                    ),
                    "$it => ($it.DateTimeProp != $it.DateTimeProp)"
                ),
                new DateInEqualitiesTheoryData
                (
                    new GreaterThanOrEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("DateTimeProp", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("DateTimeProp", new ParameterDescriptor(parameterName))
                    ),
                    "$it => ($it.DateTimeProp >= $it.DateTimeProp)"
                ),
                new DateInEqualitiesTheoryData
                (
                    new LessThanOrEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("DateTimeProp", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("DateTimeProp", new ParameterDescriptor(parameterName))
                    ),
                    "$it => ($it.DateTimeProp <= $it.DateTimeProp)"
                )
            ];

        [Theory]
        [MemberData(nameof(DateInEqualities_Data), MemberType = typeof(FilterDescriptorTests))]
        public void DateInEqualities(DateInEqualitiesTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<DataTypes>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
            {
                return GetFilter<T>
                (
                    theoryData.FilterBody
                );
            }
        }
        #endregion Inequalities

        #region Logical Operators
        [Fact]
        public void BooleanOperatorNullableTypes()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (($it.UnitPrice == Convert(5.00)) OrElse ($it.CategoryID == 0))");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new OrBinaryDescriptor
                    (
                        new EqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(5.00m, typeof(decimal).AssemblyQualifiedName)
                        ),
                        new EqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("CategoryID", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(0, typeof(int).AssemblyQualifiedName)
                        )
                    )
                );
        }

        [Fact]
        public void BooleanComparisonOnNullableAndNonNullableType()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Discontinued == Convert(True))");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("Discontinued", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(true, typeof(bool).AssemblyQualifiedName)
                    )
                );
        }

        [Fact]
        public void BooleanComparisonOnNullableType()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Discontinued == $it.Discontinued)");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("Discontinued", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("Discontinued", new ParameterDescriptor(parameterName))
                    )
                );
        }

        [Theory]
        [InlineData(null, null, false)]
        [InlineData(5.0, 0, true)]
        [InlineData(null, 1, false)]
        public void OrDescriptor(object unitPrice, object unitsInStock, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice), UnitsInStock = ToNullable<short>(unitsInStock) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice == Convert({0:0.00})) OrElse (Convert($it.UnitsInStock) == Convert({1})))", 5.0, 0));
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new OrBinaryDescriptor
                    (
                        new EqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(5.00m, typeof(decimal).AssemblyQualifiedName)
                        ),
                        new EqualsBinaryDescriptor
                        (
                            new ConvertDescriptor(new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)), typeof(int?).AssemblyQualifiedName),
                            new ConstantDescriptor(0, typeof(int).AssemblyQualifiedName)
                        )
                    )
                );
        }

        [Theory]
        [InlineData(null, null, false)]
        [InlineData(5.0, 10, true)]
        [InlineData(null, 1, false)]
        public void AndDescriptor(object unitPrice, object unitsInStock, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice), UnitsInStock = ToNullable<short>(unitsInStock) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice == Convert({0:0.00})) AndAlso (Convert($it.UnitsInStock) == Convert({1:0.00})))", 5.0, 10.0));
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AndBinaryDescriptor
                    (
                        new EqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(5.00m, typeof(decimal).AssemblyQualifiedName)
                        ),
                        new EqualsBinaryDescriptor
                        (
                            new ConvertDescriptor(new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)), typeof(decimal?).AssemblyQualifiedName),
                            new ConstantDescriptor(10.00m, typeof(decimal).AssemblyQualifiedName)
                        )
                    )
                );
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData(5.0, false)]
        [InlineData(5.5, true)]
        public void Negation(object unitPrice, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => Not(($it.UnitPrice == Convert({0:0.00})))", 5.0));
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new NotDescriptor
                    (
                        new EqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(5.00m, typeof(decimal).AssemblyQualifiedName)
                        )
                    )
                );
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, true)]
        public void BoolNegation(bool discontinued, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { Discontinued = discontinued });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => Convert(Not($it.Discontinued))");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new NotDescriptor
                    (
                        new MemberSelectorDescriptor("Discontinued", new ParameterDescriptor(parameterName))
                    )
                );
        }

        [Fact]
        public void NestedNegation()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => Convert(Not(Not(Not($it.Discontinued))))");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new NotDescriptor
                    (
                        new NotDescriptor
                        (
                            new NotDescriptor
                            (
                                new MemberSelectorDescriptor("Discontinued", new ParameterDescriptor(parameterName))
                            )
                        )
                    )
                );
        }
        #endregion Logical Operators

        #region Arithmetic Operators
        [Theory]
        [InlineData(null, false)]
        [InlineData(5.0, true)]
        [InlineData(15.01, false)]
        public void Subtraction(object unitPrice, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(unitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice - Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0));
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new LessThanBinaryDescriptor
                    (
                        new SubtractBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(1.00m, typeof(decimal).AssemblyQualifiedName)
                        ),
                        new ConstantDescriptor(5.00m, typeof(decimal).AssemblyQualifiedName)
                    )
                );
        }

        [Fact]
        public void Addition()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice + Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new LessThanBinaryDescriptor
                    (
                        new AddBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(1.00m, typeof(decimal).AssemblyQualifiedName)
                        ),
                        new ConstantDescriptor(5.00m, typeof(decimal).AssemblyQualifiedName)
                    )
                );
        }

        [Fact]
        public void Multiplication()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice * Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new LessThanBinaryDescriptor
                    (
                        new MultiplyBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(1.00m, typeof(decimal).AssemblyQualifiedName)
                        ),
                        new ConstantDescriptor(5.00m, typeof(decimal).AssemblyQualifiedName)
                    )
                );
        }

        [Fact]
        public void Division()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice / Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new LessThanBinaryDescriptor
                    (
                        new DivideBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(1.00m, typeof(decimal).AssemblyQualifiedName)
                        ),
                        new ConstantDescriptor(5.00m, typeof(decimal).AssemblyQualifiedName)
                    )
                );
        }

        [Fact]
        public void Modulo()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.UnitPrice % Convert({0:0.00})) < Convert({1:0.00}))", 1.0, 5.0));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new LessThanBinaryDescriptor
                    (
                        new ModuloBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(1.00m, typeof(decimal).AssemblyQualifiedName)
                        ),
                        new ConstantDescriptor(5.00m, typeof(decimal).AssemblyQualifiedName)
                    )
                );
        }
        #endregion Arithmetic Operators

        #region NULL handling
        public class NullHandlingTheoryData(DescriptorBase filterBody, object unitsInStock, object unitsOnOrder, bool expectedResult)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public object UnitsInStock { get; } = unitsInStock;
            public object UnitsOnOrder { get; } = unitsOnOrder;
            public bool ExpectedResult { get; } = expectedResult;
        }

        public static TheoryData<NullHandlingTheoryData> NullHandling_Data
            =>
            [
                new NullHandlingTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("UnitsOnOrder", new ParameterDescriptor(parameterName))
                    ),
                    null,
                    null,
                    true
                ),
                new NullHandlingTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("UnitsOnOrder", new ParameterDescriptor(parameterName))
                    ),
                    null,
                    null,
                    false
                ),
                new NullHandlingTheoryData
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("UnitsOnOrder", new ParameterDescriptor(parameterName))
                    ),
                    null,
                    null,
                    false
                ),
                new NullHandlingTheoryData
                (
                    new GreaterThanOrEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("UnitsOnOrder", new ParameterDescriptor(parameterName))
                    ),
                    null,
                    null,
                    false
                ),
                new NullHandlingTheoryData
                (
                    new LessThanBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("UnitsOnOrder", new ParameterDescriptor(parameterName))
                    ),
                    null,
                    null,
                    false
                ),
                new NullHandlingTheoryData
                (
                    new LessThanOrEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("UnitsOnOrder", new ParameterDescriptor(parameterName))
                    ),
                    null,
                    null,
                    false
                ),
                new NullHandlingTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new AddBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)),
                            new MemberSelectorDescriptor("UnitsOnOrder", new ParameterDescriptor(parameterName))
                        ),
                        new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName))
                    ),
                    null,
                    null,
                    true
                ),
                new NullHandlingTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new SubtractBinaryDescriptor

                        (
                            new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)),
                            new MemberSelectorDescriptor("UnitsOnOrder", new ParameterDescriptor(parameterName))
                        ),
                        new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName))
                    ),
                    null,
                    null,
                    true
                ),
                new NullHandlingTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MultiplyBinaryDescriptor

                        (
                            new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)),
                            new MemberSelectorDescriptor("UnitsOnOrder", new ParameterDescriptor(parameterName))
                        ),
                        new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName))
                    ),
                    null,
                    null,
                    true
                ),
                new NullHandlingTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new DivideBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)),
                            new MemberSelectorDescriptor("UnitsOnOrder", new ParameterDescriptor(parameterName))
                        ),
                        new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName))
                    ),
                    null,
                    null,
                    true
                ),
                new NullHandlingTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ModuloBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)),
                            new MemberSelectorDescriptor("UnitsOnOrder", new ParameterDescriptor(parameterName))
                        ),
                        new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName))
                    ),
                    null,
                    null,
                    true
                ),
                new NullHandlingTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("UnitsOnOrder", new ParameterDescriptor(parameterName))
                    ),
                    1,
                    null,
                    false
                ),
                new NullHandlingTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("UnitsOnOrder", new ParameterDescriptor(parameterName))
                    ),
                    1,
                    1,
                    true
                )
            ];

        [Theory]
        [MemberData(nameof(NullHandling_Data), MemberType = typeof(FilterDescriptorTests))]
        public void NullHandling(NullHandlingTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { UnitsInStock = ToNullable<short>(theoryData.UnitsInStock), UnitsOnOrder = ToNullable<short>(theoryData.UnitsOnOrder) });

            //assert
            Assert.Equal(theoryData.ExpectedResult, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class NullHandling_LiteralNullTheoryData(DescriptorBase filterBody, object unitsInStock, bool expectedResult)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public object UnitsInStock { get; } = unitsInStock;
            public bool ExpectedResult { get; } = expectedResult;
        }

        public static TheoryData<NullHandling_LiteralNullTheoryData> NullHandling_LiteralNull_Data
            =>
            [
                new NullHandling_LiteralNullTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(null)
                    ),
                    null,
                    true
                ),
                new NullHandling_LiteralNullTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("UnitsInStock", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(null)
                    ),
                    null,
                    false
                )
            ];

        [Theory]
        [MemberData(nameof(NullHandling_LiteralNull_Data), MemberType = typeof(FilterDescriptorTests))]
        public void NullHandling_LiteralNull(NullHandling_LiteralNullTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { UnitsInStock = ToNullable<short>(theoryData.UnitsInStock) });

            //assert
            Assert.Equal(theoryData.ExpectedResult, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }
        #endregion NULL handling

        public class ComparisonsInvolvingCastsAndNullableValuesTheoryData(DescriptorBase filterBody)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
        }

        public static TheoryData<ComparisonsInvolvingCastsAndNullableValuesTheoryData> ComparisonsInvolvingCastsAndNullableValues_Data
            =>
            [
                new ComparisonsInvolvingCastsAndNullableValuesTheoryData
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new IndexOfDescriptor
                        (
                            new ConstantDescriptor("hello"),
                            new MemberSelectorDescriptor("StringProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertDescriptor

                        (
                            new MemberSelectorDescriptor("UIntProp", new ParameterDescriptor(parameterName)),
                            typeof(int?).AssemblyQualifiedName
                        )
                    )
                ),
                new ComparisonsInvolvingCastsAndNullableValuesTheoryData
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new IndexOfDescriptor
                        (
                            new ConstantDescriptor("hello"),
                            new MemberSelectorDescriptor("StringProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertDescriptor
                        (
                            new MemberSelectorDescriptor("ULongProp", new ParameterDescriptor(parameterName)),
                            typeof(int?).AssemblyQualifiedName
                        )
                    )
                ),
                new ComparisonsInvolvingCastsAndNullableValuesTheoryData
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new IndexOfDescriptor
                        (
                            new ConstantDescriptor("hello"),
                            new MemberSelectorDescriptor("StringProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertDescriptor
                        (
                            new MemberSelectorDescriptor("UShortProp", new ParameterDescriptor(parameterName)),
                            typeof(int?).AssemblyQualifiedName
                        )
                    )
                ),
                new ComparisonsInvolvingCastsAndNullableValuesTheoryData
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new IndexOfDescriptor
                        (
                            new ConstantDescriptor("hello"),
                            new MemberSelectorDescriptor("StringProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertDescriptor
                        (
                            new MemberSelectorDescriptor("NullableUShortProp", new ParameterDescriptor(parameterName)),
                            typeof(int?).AssemblyQualifiedName
                        )
                    )
                ),
                new ComparisonsInvolvingCastsAndNullableValuesTheoryData
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new IndexOfDescriptor
                        (
                            new ConstantDescriptor("hello"),
                            new MemberSelectorDescriptor("StringProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertDescriptor
                        (
                            new MemberSelectorDescriptor("NullableUIntProp", new ParameterDescriptor(parameterName)),
                            typeof(int?).AssemblyQualifiedName
                        )
                    )
                ),
                new ComparisonsInvolvingCastsAndNullableValuesTheoryData
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new IndexOfDescriptor
                        (
                            new ConstantDescriptor("hello"),
                            new MemberSelectorDescriptor("StringProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertDescriptor
                        (
                            new MemberSelectorDescriptor("NullableULongProp", new ParameterDescriptor(parameterName)),
                            typeof(int?).AssemblyQualifiedName
                        )
                    )
                )
            ];

        [Theory]
        [MemberData(nameof(ComparisonsInvolvingCastsAndNullableValues_Data), MemberType = typeof(FilterDescriptorTests))]
        public void ComparisonsInvolvingCastsAndNullableValues(ComparisonsInvolvingCastsAndNullableValuesTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<DataTypes>();

            //assert
            Assert.Throws<ArgumentNullException>(() => RunFilter(filter, new DataTypes()));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        [Theory]
        [InlineData(null, null, true)]
        [InlineData("not doritos", 0, true)]
        [InlineData("Doritos", 1, false)]
        public void Grouping(string productName, object unitsInStock, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = productName, UnitsInStock = ToNullable<short>(unitsInStock) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (($it.ProductName != \"Doritos\") OrElse ($it.UnitPrice < Convert({0:0.00})))", 5.0));
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new OrBinaryDescriptor
                    (
                        new NotEqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor("Doritos")
                        ),
                        new LessThanBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(5.00m, typeof(decimal).AssemblyQualifiedName)
                        )
                    )
                );
        }

        [Fact]
        public void MemberExpressions()
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { Category = new Category { CategoryName = "Snacks" } });

            //assert
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));
            AssertFilterStringIsCorrect(filter, "$it => ($it.Category.CategoryName == \"Snacks\")");
            Assert.True(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "CategoryName",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("Snacks")
                    )
                );
        }

        [Fact]
        public void MemberExpressionsRecursive()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));
            AssertFilterStringIsCorrect(filter, "$it => ($it.Category.Product.Category.CategoryName == \"Snacks\")");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "CategoryName",
                            new MemberSelectorDescriptor
                            (
                                "Category",
                                new MemberSelectorDescriptor

                                (
                                    "Product",
                                    new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                                )
                            )
                        ),
                        new ConstantDescriptor("Snacks")
                    )
                );
        }

        [Fact]
        public void ComplexPropertyNavigation()
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { SupplierAddress = new Address { City = "Redmond" } });

            //assert
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));
            AssertFilterStringIsCorrect(filter, "$it => ($it.SupplierAddress.City == \"Redmond\")");
            Assert.True(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "City",
                            new MemberSelectorDescriptor("SupplierAddress", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("Redmond")
                    )
                );
        }

        #region Any/All
        [Fact]
        public void AnyOnNavigationEnumerableCollections()
        {
            //act
            var filter = CreateFilter<Product>();

            bool result1 = RunFilter
            (
                filter,
                new Product
                {
                    Category = new Category
                    {
                        EnumerableProducts =
                        [
                            new Product { ProductName = "Snacks" },
                            new Product { ProductName = "NonSnacks" }
                        ]
                    }
                }
            );

            bool result2 = RunFilter
            (
                filter,
                new Product
                {
                    Category = new Category
                    {
                        EnumerableProducts =
                        [
                            new Product { ProductName = "NonSnacks" }
                        ]
                    }
                }
            );

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.EnumerableProducts.Any(P => (P.ProductName == \"Snacks\"))");
            Assert.True(result1);
            Assert.False(result2);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "EnumerableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new EqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor("P")),
                            new ConstantDescriptor("Snacks")
                        ),
                        "P"
                    )
                );
        }

        [Fact]
        public void AnyOnNavigationQueryableCollections()
        {
            //act
            var filter = CreateFilter<Product>();

            bool result1 = RunFilter
            (
                filter,
                new Product
                {
                    Category = new Category
                    {
                        QueryableProducts = new Product[]
                        {
                            new() { ProductName = "Snacks" },
                            new() { ProductName = "NonSnacks" }
                        }.AsQueryable()
                    }
                }
            );

            bool result2 = RunFilter
            (
                filter,
                new Product
                {
                    Category = new Category
                    {
                        QueryableProducts = new Product[]
                        {
                            new() { ProductName = "NonSnacks" }
                        }.AsQueryable()
                    }
                }
            );

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.QueryableProducts.Any(P => (P.ProductName == \"Snacks\"))");
            Assert.True(result1);
            Assert.False(result2);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "QueryableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new EqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor("P")),
                            new ConstantDescriptor("Snacks")
                        ),
                        "P"
                    )
                );
        }

        public class AnyInOnNavigationTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<AnyInOnNavigationTheoryData> AnyInOnNavigation_Data
            =>
            [
                new AnyInOnNavigationTheoryData
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "QueryableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new InDescriptor
                        (
                            new MemberSelectorDescriptor("ProductID", new ParameterDescriptor("P")),
                            new CollectionConstantDescriptor
                            (
                                [1],
                                typeof(int).AssemblyQualifiedName
                            )
                        ),
                        "P"
                    ),
                    "$it => $it.Category.QueryableProducts.Any(P => System.Collections.Generic.List`1[System.Int32].Contains(P.ProductID))"
                ),
                new AnyInOnNavigationTheoryData
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "EnumerableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new InDescriptor
                        (
                            new MemberSelectorDescriptor("ProductID", new ParameterDescriptor("P")),
                            new CollectionConstantDescriptor
                            (
                                [1],
                                typeof(int).AssemblyQualifiedName
                            )
                        ),
                        "P"
                    ),
                    "$it => $it.Category.EnumerableProducts.Any(P => System.Collections.Generic.List`1[System.Int32].Contains(P.ProductID))"
                ),
                new AnyInOnNavigationTheoryData
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "QueryableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new InDescriptor
                        (
                            new MemberSelectorDescriptor("GuidProperty", new ParameterDescriptor("P")),
                            new CollectionConstantDescriptor
                            (
                                [new Guid("dc75698b-581d-488b-9638-3e28dd51d8f7")],
                                typeof(Guid).AssemblyQualifiedName
                            )
                        ),
                        "P"
                    ),
                    "$it => $it.Category.QueryableProducts.Any(P => System.Collections.Generic.List`1[System.Guid].Contains(P.GuidProperty))"
                ),
                new AnyInOnNavigationTheoryData
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "EnumerableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new InDescriptor
                        (
                            new MemberSelectorDescriptor("GuidProperty", new ParameterDescriptor("P")),
                            new CollectionConstantDescriptor
                            (
                                [new Guid("dc75698b-581d-488b-9638-3e28dd51d8f7")],
                                typeof(Guid).AssemblyQualifiedName
                            )
                        ),
                        "P"
                    ),
                    "$it => $it.Category.EnumerableProducts.Any(P => System.Collections.Generic.List`1[System.Guid].Contains(P.GuidProperty))"
                ),
                new AnyInOnNavigationTheoryData
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "QueryableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new InDescriptor
                        (
                            new MemberSelectorDescriptor("NullableGuidProperty", new ParameterDescriptor("P")),
                            new CollectionConstantDescriptor
                            (
                                [new Guid("dc75698b-581d-488b-9638-3e28dd51d8f7")],
                                typeof(Guid?).AssemblyQualifiedName
                            )
                        ),
                        "P"
                    ),
                    "$it => $it.Category.QueryableProducts.Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Guid]].Contains(P.NullableGuidProperty))"
                ),
                new AnyInOnNavigationTheoryData
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "EnumerableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new InDescriptor
                        (
                            new MemberSelectorDescriptor("NullableGuidProperty", new ParameterDescriptor("P")),
                            new CollectionConstantDescriptor
                            (
                                [new Guid("dc75698b-581d-488b-9638-3e28dd51d8f7")],
                                typeof(Guid?).AssemblyQualifiedName
                            )
                        ),
                        "P"
                    ),
                    "$it => $it.Category.EnumerableProducts.Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Guid]].Contains(P.NullableGuidProperty))"
                ),
                new AnyInOnNavigationTheoryData
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "QueryableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new InDescriptor
                        (
                            new MemberSelectorDescriptor("Discontinued", new ParameterDescriptor("P")),
                            new CollectionConstantDescriptor
                            (
                                [false, null],
                                typeof(bool?).AssemblyQualifiedName
                            )
                        ),
                        "P"
                    ),
                    "$it => $it.Category.QueryableProducts.Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Boolean]].Contains(P.Discontinued))"
                ),
                new AnyInOnNavigationTheoryData
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "EnumerableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new InDescriptor
                        (
                            new MemberSelectorDescriptor("Discontinued", new ParameterDescriptor("P")),
                            new CollectionConstantDescriptor
                            (
                                [false, null],
                                typeof(bool?).AssemblyQualifiedName
                            )
                        ),
                        "P"
                    ),
                    "$it => $it.Category.EnumerableProducts.Any(P => System.Collections.Generic.List`1[System.Nullable`1[System.Boolean]].Contains(P.Discontinued))"
                )
            ];

        [Theory]
        [MemberData(nameof(AnyInOnNavigation_Data), MemberType = typeof(FilterDescriptorTests))]
        public void AnyInOnNavigation(AnyInOnNavigationTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class AnyOnNavigation_ContradictionTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<AnyOnNavigation_ContradictionTheoryData> AnyOnNavigation_Contradiction_Data
            =>
            [
                new AnyOnNavigation_ContradictionTheoryData
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "QueryableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(false),
                        "P"
                    ),
                    "$it => $it.Category.QueryableProducts.Any(P => False)"
                ),
                new AnyOnNavigation_ContradictionTheoryData
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "QueryableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new AndBinaryDescriptor
                        (
                            new ConstantDescriptor(false),
                            new EqualsBinaryDescriptor
                            (
                                new MemberSelectorDescriptor("ProductName", new ParameterDescriptor("P")),
                                new ConstantDescriptor("Snacks")
                            )
                        ),
                        "P"
                    ),
                    "$it => $it.Category.QueryableProducts.Any(P => (False AndAlso (P.ProductName == \"Snacks\")))"
                ),
                new AnyOnNavigation_ContradictionTheoryData
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "QueryableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        )
                    ),
                    "$it => $it.Category.QueryableProducts.Any()"
                )
            ];

        [Theory]
        [MemberData(nameof(AnyOnNavigation_Contradiction_Data), MemberType = typeof(FilterDescriptorTests))]
        public void AnyOnNavigation_Contradiction(AnyOnNavigation_ContradictionTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        [Fact]
        public void AnyOnNavigation_NullCollection()
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter
            (
                filter,
                new Product
                {
                    Category = new Category
                    {
                        EnumerableProducts =
                        [
                            new Product { ProductName = "Snacks" }
                        ]
                    }
                }
            );

            //assert
            Assert.Throws<ArgumentNullException>(() => RunFilter(filter, new Product { Category = new Category { } }));
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.EnumerableProducts.Any(P => (P.ProductName == \"Snacks\"))");
            Assert.True(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "EnumerableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new EqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor("P")),
                            new ConstantDescriptor("Snacks")
                        ),
                        "P"
                    )
                );
        }


        [Fact]
        public void AllOnNavigation_NullCollection()
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter
            (
                filter,
                new Product
                {
                    Category = new Category
                    {
                        EnumerableProducts =
                        [
                            new Product { ProductName = "Snacks" }
                        ]
                    }
                }
            );

            //assert
            Assert.Throws<ArgumentNullException>(() => RunFilter(filter, new Product { Category = new Category { } }));
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.EnumerableProducts.All(P => (P.ProductName == \"Snacks\"))");
            Assert.True(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AllDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "EnumerableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new EqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor("P")),
                            new ConstantDescriptor("Snacks")
                        ),
                        "P"
                    )
                );
        }

        [Fact]
        public void MultipleAnys_WithSameRangeVariableName()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.AlternateIDs.Any(n => (n == 42)) AndAlso $it.AlternateAddresses.Any(n => (n.City == \"Redmond\")))");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AndBinaryDescriptor
                    (
                        new AnyDescriptor
                        (
                            new MemberSelectorDescriptor("AlternateIDs", new ParameterDescriptor(parameterName)),
                            new EqualsBinaryDescriptor
                            (
                                new ParameterDescriptor("n"),
                                new ConstantDescriptor(42)
                            ),
                            "n"
                        ),
                        new AnyDescriptor
                        (
                            new MemberSelectorDescriptor("AlternateAddresses", new ParameterDescriptor(parameterName)),
                            new EqualsBinaryDescriptor
                            (
                                new MemberSelectorDescriptor("City", new ParameterDescriptor("n")),
                                new ConstantDescriptor("Redmond")
                            ),
                            "n"
                        )
                    )
                );
        }

        [Fact]
        public void MultipleAlls_WithSameRangeVariableName()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.AlternateIDs.All(n => (n == 42)) AndAlso $it.AlternateAddresses.All(n => (n.City == \"Redmond\")))");
            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AndBinaryDescriptor
                    (
                        new AllDescriptor
                        (
                            new MemberSelectorDescriptor("AlternateIDs", new ParameterDescriptor(parameterName)),
                            new EqualsBinaryDescriptor
                            (
                                new ParameterDescriptor("n"),
                                new ConstantDescriptor(42)
                            ),
                            "n"
                        ),
                        new AllDescriptor
                        (
                            new MemberSelectorDescriptor("AlternateAddresses", new ParameterDescriptor(parameterName)),
                            new EqualsBinaryDescriptor
                            (
                                new MemberSelectorDescriptor("City", new ParameterDescriptor("n")),
                                new ConstantDescriptor("Redmond")
                            ),
                            "n"
                        )
                    )
                );
        }

        [Fact]
        public void AnyOnNavigationEnumerableCollections_EmptyFilter()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.EnumerableProducts.Any()");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "EnumerableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        )
                    )
                );
        }

        [Fact]
        public void AnyOnNavigationQueryableCollections_EmptyFilter()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.QueryableProducts.Any()");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "QueryableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        )
                    )
                );
        }

        [Fact]
        public void AllOnNavigationEnumerableCollections()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.EnumerableProducts.All(P => (P.ProductName == \"Snacks\"))");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AllDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "EnumerableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new EqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor("P")),
                            new ConstantDescriptor("Snacks")
                        ),
                        "P"
                    )
                );
        }

        [Fact]
        public void AllOnNavigationQueryableCollections()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.QueryableProducts.All(P => (P.ProductName == \"Snacks\"))");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AllDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "QueryableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new EqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor("P")),
                            new ConstantDescriptor("Snacks")
                        ),
                        "P"
                    )
                );
        }

        [Fact]
        public void AnyInSequenceNotNested()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Category.QueryableProducts.Any(P => (P.ProductName == \"Snacks\")) OrElse $it.Category.QueryableProducts.Any(P2 => (P2.ProductName == \"Snacks\")))");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new OrBinaryDescriptor
                    (
                        new AnyDescriptor
                        (
                            new MemberSelectorDescriptor
                            (
                                "QueryableProducts",
                                new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                            ),
                            new EqualsBinaryDescriptor
                            (
                                new MemberSelectorDescriptor("ProductName", new ParameterDescriptor("P")),
                                new ConstantDescriptor("Snacks")
                            ),
                            "P"
                        ),
                        new AnyDescriptor
                        (
                            new MemberSelectorDescriptor
                            (
                                "QueryableProducts",
                                new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                            ),
                            new EqualsBinaryDescriptor
                            (
                                new MemberSelectorDescriptor("ProductName", new ParameterDescriptor("P2")),
                                new ConstantDescriptor("Snacks")
                            ),
                            "P2"
                        )
                    )
                );
        }

        [Fact]
        public void AllInSequenceNotNested()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Category.QueryableProducts.All(P => (P.ProductName == \"Snacks\")) OrElse $it.Category.QueryableProducts.All(P2 => (P2.ProductName == \"Snacks\")))");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new OrBinaryDescriptor
                    (
                        new AllDescriptor
                        (
                            new MemberSelectorDescriptor
                            (
                                "QueryableProducts",
                                new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                            ),
                            new EqualsBinaryDescriptor
                            (
                                new MemberSelectorDescriptor("ProductName", new ParameterDescriptor("P")),
                                new ConstantDescriptor("Snacks")
                            ),
                            "P"
                        ),
                        new AllDescriptor
                        (
                            new MemberSelectorDescriptor
                            (
                                "QueryableProducts",
                                new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                            ),
                            new EqualsBinaryDescriptor
                            (
                                new MemberSelectorDescriptor("ProductName", new ParameterDescriptor("P2")),
                                new ConstantDescriptor("Snacks")
                            ),
                            "P2"
                        )
                    )
                );
        }

        [Fact]
        public void AnyOnPrimitiveCollection()
        {
            //act
            var filter = CreateFilter<Product>();

            bool result1 = RunFilter
            (
                filter,
                new Product { AlternateIDs = [1, 2, 42] }
            );

            bool result2 = RunFilter
            (
                filter,
                new Product { AlternateIDs = [1, 2] }
            );

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.AlternateIDs.Any(id => (id == 42))");
            Assert.True(result1);
            Assert.False(result2);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor("AlternateIDs", new ParameterDescriptor(parameterName)),
                        new EqualsBinaryDescriptor
                        (
                            new ParameterDescriptor("id"),
                            new ConstantDescriptor(42)
                        ),
                        "id"
                    )
                );
        }

        [Fact]
        public void AllOnPrimitiveCollection()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.AlternateIDs.All(id => (id == 42))");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AllDescriptor
                    (
                        new MemberSelectorDescriptor("AlternateIDs", new ParameterDescriptor(parameterName)),
                        new EqualsBinaryDescriptor
                        (
                            new ParameterDescriptor("id"),
                            new ConstantDescriptor(42)
                        ),
                        "id"
                    )
                );
        }

        [Fact]
        public void AnyOnComplexCollection()
        {
            //act
            var filter = CreateFilter<Product>();

            bool result = RunFilter
            (
                filter,
                new Product { AlternateAddresses = [new Address { City = "Redmond" }] }
            );

            //assert
            Assert.Throws<ArgumentNullException>(() => RunFilter(filter, new Product { }));
            AssertFilterStringIsCorrect(filter, "$it => $it.AlternateAddresses.Any(address => (address.City == \"Redmond\"))");
            Assert.True(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AnyDescriptor
                    (
                        new MemberSelectorDescriptor("AlternateAddresses", new ParameterDescriptor(parameterName)),
                        new EqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("City", new ParameterDescriptor("address")),
                            new ConstantDescriptor("Redmond")
                        ),
                        "address"
                    )
                );
        }

        [Fact]
        public void AllOnComplexCollection()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.AlternateAddresses.All(address => (address.City == \"Redmond\"))");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AllDescriptor
                    (
                        new MemberSelectorDescriptor("AlternateAddresses", new ParameterDescriptor(parameterName)),
                        new EqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("City", new ParameterDescriptor("address")),
                            new ConstantDescriptor("Redmond")
                        ),
                        "address"
                    )
                );
        }

        [Fact]
        public void RecursiveAllAny()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.QueryableProducts.All(P => P.Category.EnumerableProducts.Any(PP => (PP.ProductName == \"Snacks\")))");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AllDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "QueryableProducts",
                            new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                        ),
                        new AnyDescriptor
                        (
                            new MemberSelectorDescriptor
                            (
                                "EnumerableProducts",
                                new MemberSelectorDescriptor("Category", new ParameterDescriptor("P"))
                            ),
                            new EqualsBinaryDescriptor
                            (
                                new MemberSelectorDescriptor("ProductName", new ParameterDescriptor("PP")),
                                new ConstantDescriptor("Snacks")
                            ),
                            "PP"
                        ),
                        "P"
                    )
                );
        }
        #endregion Any/All

        #region String Functions
        [Theory]
        [InlineData("Abcd", 0, "Abcd", true)]
        [InlineData("Abcd", 1, "bcd", true)]
        [InlineData("Abcd", 3, "d", true)]
        [InlineData("Abcd", 4, "", true)]
        public void StringSubstringStart(string productName, int startIndex, string compareString, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter
            (
                filter,
                new Product { ProductName = productName }
            );

            //assert
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new SubstringDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(startIndex)
                        ),
                        new ConstantDescriptor(compareString)
                    )
                );
        }

        [Theory]
        [InlineData("Abcd", -1, "Abcd")]
        [InlineData("Abcd", 5, "")]
        public void StringSubstringStartOutOfRange(string productName, int startIndex, string compareString)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            Assert.Throws<ArgumentOutOfRangeException>(() => RunFilter(filter, new Product { ProductName = productName }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new SubstringDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(startIndex)
                        ),
                        new ConstantDescriptor(compareString)
                    )
                );
        }

        [Theory]
        [InlineData("Abcd", 0, 1, "A", true)]
        [InlineData("Abcd", 0, 4, "Abcd", true)]
        [InlineData("Abcd", 0, 3, "Abc", true)]
        [InlineData("Abcd", 1, 3, "bcd", true)]
        [InlineData("Abcd", 2, 1, "c", true)]
        [InlineData("Abcd", 3, 1, "d", true)]
        public void StringSubstringStartAndLength(string productName, int startIndex, int length, string compareString, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter
            (
                filter,
                new Product { ProductName = productName }
            );

            //assert
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new SubstringDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(startIndex),
                            new ConstantDescriptor(length)
                        ),
                        new ConstantDescriptor(compareString)
                    )
                );
        }

        [Theory]
        [InlineData("Abcd", -1, 4, "Abcd")]
        [InlineData("Abcd", -1, 3, "Abc")]
        [InlineData("Abcd", 0, 5, "Abcd")]
        [InlineData("Abcd", 1, 5, "bcd")]
        [InlineData("Abcd", 4, 1, "")]
        [InlineData("Abcd", 0, -1, "")]
        [InlineData("Abcd", 5, -1, "")]
        public void StringSubstringStartAndLengthOutOfRange(string productName, int startIndex, int length, string compareString)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            Assert.Throws<ArgumentOutOfRangeException>(() => RunFilter(filter, new Product { ProductName = productName }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new SubstringDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(startIndex),
                            new ConstantDescriptor(length)
                        ),
                        new ConstantDescriptor(compareString)
                    )
                );
        }

        [Theory]
        [InlineData("Abcd", true)]
        [InlineData("Abd", false)]
        public void StringContains(string productName, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.ProductName.Contains(\"Abc\")");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new ContainsDescriptor
                    (
                        new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor("Abc")
                    )
                );
        }

        [Fact]
        public void StringContainsNullReferenceException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.ProductName.Contains(\"Abc\")");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new ContainsDescriptor
                    (
                        new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor("Abc")
                    )
                );
        }

        [Theory]
        [InlineData("Abcd", true)]
        [InlineData("Abd", false)]
        public void StringStartsWith(string productName, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.ProductName.StartsWith(\"Abc\")");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new StartsWithDescriptor
                    (
                        new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor("Abc")
                    )
                );
        }

        [Fact]
        public void StringStartsWithNullReferenceException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.ProductName.StartsWith(\"Abc\")");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new StartsWithDescriptor
                    (
                        new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor("Abc")
                    )
                );
        }

        [Theory]
        [InlineData("AAbc", true)]
        [InlineData("Abcd", false)]
        public void StringEndsWith(string productName, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.ProductName.EndsWith(\"Abc\")");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EndsWithDescriptor
                    (
                        new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor("Abc")
                    )
                );
        }

        [Fact]
        public void StringEndsWithNullReferenceException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.ProductName.EndsWith(\"Abc\")");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EndsWithDescriptor
                    (
                        new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor("Abc")
                    )
                );
        }

        [Theory]
        [InlineData("AAbc", true)]
        [InlineData("", false)]
        public void StringLength(string productName, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.Length > 0)");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new LengthDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(0)
                    )
                );
        }

        [Fact]
        public void StringLengthNullReferenceException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.Length > 0)");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new LengthDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(0)
                    )
                );
        }

        [Theory]
        [InlineData("12345Abc", true)]
        [InlineData("1234Abc", false)]
        public void StringIndexOf(string productName, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.IndexOf(\"Abc\") == 5)");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new IndexOfDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor("Abc")
                        ),
                        new ConstantDescriptor(5)
                    )
                );
        }

        [Fact]
        public void StringIndexOfNullReferenceException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.IndexOf(\"Abc\") == 5)");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new IndexOfDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor("Abc")
                        ),
                        new ConstantDescriptor(5)
                    )
                );
        }

        [Theory]
        [InlineData("123uctName", true)]
        [InlineData("1234Abc", false)]
        public void StringSubstring(string productName, bool expected)
        {
            //act
            var filter1 = CreateFilter1<Product>();
            var filter2 = CreateFilter2<Product>();
            bool result = RunFilter(filter1, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter1, "$it => ($it.ProductName.Substring(3) == \"uctName\")");
            AssertFilterStringIsCorrect(filter2, "$it => ($it.ProductName.Substring(3, 4) == \"uctN\")");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter1<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new SubstringDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(3)
                        ),
                        new ConstantDescriptor("uctName")
                    )
                );

            Expression<Func<T, bool>> CreateFilter2<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new SubstringDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(3),
                            new ConstantDescriptor(4)
                        ),
                        new ConstantDescriptor("uctN")
                    )
                );
        }

        [Fact]
        public void StringSubstringNullReferenceException()
        {
            //act
            var filter1 = CreateFilter1<Product>();
            var filter2 = CreateFilter2<Product>();

            //assert
            AssertFilterStringIsCorrect(filter1, "$it => ($it.ProductName.Substring(3) == \"uctName\")");
            AssertFilterStringIsCorrect(filter2, "$it => ($it.ProductName.Substring(3, 4) == \"uctN\")");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter1, new Product { }));

            Expression<Func<T, bool>> CreateFilter1<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new SubstringDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(3)
                        ),
                        new ConstantDescriptor("uctName")
                    )
                );

            Expression<Func<T, bool>> CreateFilter2<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new SubstringDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(3),
                            new ConstantDescriptor(4)
                        ),
                        new ConstantDescriptor("uctN")
                    )
                );
        }

        [Theory]
        [InlineData("Tasty Treats", true)]
        [InlineData("Tasty Treatss", false)]
        public void StringToLower(string productName, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.ToLower() == \"tasty treats\")");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new ToLowerDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("tasty treats")
                    )
                );
        }

        [Fact]
        public void StringToLowerNullReferenceException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.ToLower() == \"tasty treats\")");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new ToLowerDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("tasty treats")
                    )
                );
        }

        [Theory]
        [InlineData("Tasty Treats", true)]
        [InlineData("Tasty Treatss", false)]
        public void StringToUpper(string productName, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.ToUpper() == \"TASTY TREATS\")");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new ToUpperDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("TASTY TREATS")
                    )
                );
        }

        [Fact]
        public void StringToUpperNullReferenceException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.ToUpper() == \"TASTY TREATS\")");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new ToUpperDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("TASTY TREATS")
                    )
                );
        }

        [Theory]
        [InlineData(" Tasty Treats  ", true)]
        [InlineData(" Tasty Treatss  ", false)]
        public void StringTrim(string productName, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.Trim() == \"Tasty Treats\")");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new TrimDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("Tasty Treats")
                    )
                );
        }

        [Fact]
        public void StringTrimNullReferenceException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName.Trim() == \"Tasty Treats\")");
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new TrimDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("Tasty Treats")
                    )
                );
        }

        [Fact]
        public void StringConcat()
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (\"Food\".Concat(\"Bar\") == \"FoodBar\")");
            Assert.True(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConcatDescriptor
                        (
                            new ConstantDescriptor("Food"),
                            new ConstantDescriptor("Bar")
                        ),
                        new ConstantDescriptor("FoodBar")
                    )
                );
        }
        #endregion String Functions

        #region Date Functions
        [Fact]
        public void DateDay()
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { DiscontinuedDate = new DateTime(2000, 10, 8, 0, 0, 0, DateTimeKind.Unspecified) });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.DiscontinuedDate.Value.Day == 8)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));
            Assert.True(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new DayDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(8)
                    )
                );
        }

        [Fact]
        public void DateDayNonNullable()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.NonNullableDiscontinuedDate.Day == 8)");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new DayDescriptor
                        (
                            new MemberSelectorDescriptor("NonNullableDiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(8)
                    )
                );
        }

        [Fact]
        public void DateMonth()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.DiscontinuedDate.Value.Month == 8)");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new MonthDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(8)
                    )
                );
        }

        [Fact]
        public void DateYear()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.DiscontinuedDate.Value.Year == 1974)");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new YearDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(1974)
                    )
                );
        }

        [Fact]
        public void DateHour()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.DiscontinuedDate.Value.Hour == 8)");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new HourDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(8)
                    )
                );
        }

        [Fact]
        public void DateMinute()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.DiscontinuedDate.Value.Minute == 12)");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new MinuteDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(12)
                    )
                );
        }

        [Fact]
        public void DateSecond()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.DiscontinuedDate.Value.Second == 33)");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new SecondDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(33)
                    )
                );
        }

        public class DateTimeOffsetFunctionsTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<DateTimeOffsetFunctionsTheoryData> DateTimeOffsetFunctions_Data
            =>
            [
                new DateTimeOffsetFunctionsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new YearDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedOffset", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(100)
                    ),
                    "$it => ($it.DiscontinuedOffset.Year == 100)"
                ),
                new DateTimeOffsetFunctionsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MonthDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedOffset", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(100)
                    ),
                    "$it => ($it.DiscontinuedOffset.Month == 100)"
                ),
                new DateTimeOffsetFunctionsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new DayDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedOffset", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(100)
                    ),
                    "$it => ($it.DiscontinuedOffset.Day == 100)"
                ),
                new DateTimeOffsetFunctionsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new HourDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedOffset", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(100)
                    ),
                    "$it => ($it.DiscontinuedOffset.Hour == 100)"
                ),
                new DateTimeOffsetFunctionsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MinuteDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedOffset", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(100)
                    ),
                    "$it => ($it.DiscontinuedOffset.Minute == 100)"
                ),
                new DateTimeOffsetFunctionsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new SecondDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedOffset", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(100)
                    ),
                    "$it => ($it.DiscontinuedOffset.Second == 100)"
                ),
                new DateTimeOffsetFunctionsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new NowDateTimeDescriptor(),
                        new ConstantDescriptor(new DateTimeOffset(new DateTime(2016, 11, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0)))
                    ),
                    "$it => (DateTimeOffset.UtcNow == 11/08/2016 00:00:00 +00:00)"
                ),
            ];

        [Theory]
        [MemberData(nameof(DateTimeOffsetFunctions_Data), MemberType = typeof(FilterDescriptorTests))]
        public void DateTimeOffsetFunctions(DateTimeOffsetFunctionsTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class DateTimeFunctionsTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<DateTimeFunctionsTheoryData> DateTimeFunctions_Data
            =>
            [
                new DateTimeFunctionsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new YearDescriptor
                        (
                            new MemberSelectorDescriptor("Birthday", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(100)
                    ),
                    "$it => ({0}.Year == 100)"
                ),
                new DateTimeFunctionsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MonthDescriptor
                        (
                            new MemberSelectorDescriptor("Birthday", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(100)
                    ),
                    "$it => ({0}.Month == 100)"
                ),
                new DateTimeFunctionsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new DayDescriptor
                        (
                            new MemberSelectorDescriptor("Birthday", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(100)
                    ),
                    "$it => ({0}.Day == 100)"
                ),
                new DateTimeFunctionsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new HourDescriptor
                        (
                            new MemberSelectorDescriptor("Birthday", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(100)
                    ),
                    "$it => ({0}.Hour == 100)"
                ),
                new DateTimeFunctionsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MinuteDescriptor
                        (
                            new MemberSelectorDescriptor("Birthday", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(100)
                    ),
                    "$it => ({0}.Minute == 100)"
                ),
                new DateTimeFunctionsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new SecondDescriptor
                        (
                            new MemberSelectorDescriptor("Birthday", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(100)
                    ),
                    "$it => ({0}.Second == 100)"
                ),
            ];

        [Theory]
        [MemberData(nameof(DateTimeFunctions_Data), MemberType = typeof(FilterDescriptorTests))]
        public void DateTimeFunctions(DateTimeFunctionsTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, String.Format(theoryData.ExpectedExpression, "$it.Birthday"));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class DateFunctions_NullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<DateFunctions_NullableTheoryData> DateFunctions_Nullable_Data
            =>
            [
                new DateFunctions_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new YearDescriptor
                        (
                            new MemberSelectorDescriptor("NullableDateProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(2015)
                    ),
                    "$it => ($it.NullableDateProperty.Value.Year == 2015)"
                ),
                new DateFunctions_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MonthDescriptor
                        (
                            new MemberSelectorDescriptor("NullableDateProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(12)
                    ),
                    "$it => ($it.NullableDateProperty.Value.Month == 12)"
                ),
                new DateFunctions_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new DayDescriptor
                        (
                            new MemberSelectorDescriptor("NullableDateProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(23)
                    ),
                    "$it => ($it.NullableDateProperty.Value.Day == 23)"
                ),
            ];

        [Theory]
        [MemberData(nameof(DateFunctions_Nullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void DateFunctions_Nullable(DateFunctions_NullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class DateOnlyFunctions_NullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<DateOnlyFunctions_NullableTheoryData> DateOnlyFunctions_Nullable_Data
            =>
            [
                new DateOnlyFunctions_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new YearDescriptor
                        (
                            new MemberSelectorDescriptor("NullableDateOnlyProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(2015)
                    ),
                    "$it => ($it.NullableDateOnlyProperty.Value.Year == 2015)"
                ),
                new DateOnlyFunctions_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MonthDescriptor
                        (
                            new MemberSelectorDescriptor("NullableDateOnlyProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(12)
                    ),
                    "$it => ($it.NullableDateOnlyProperty.Value.Month == 12)"
                ),
                new DateOnlyFunctions_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new DayDescriptor
                        (
                            new MemberSelectorDescriptor("NullableDateOnlyProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(23)
                    ),
                    "$it => ($it.NullableDateOnlyProperty.Value.Day == 23)"
                ),
            ];

        [Theory]
        [MemberData(nameof(DateOnlyFunctions_Nullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void DateOnlyFunctions_Nullable(DateOnlyFunctions_NullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class DateFunctions_NonNullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<DateFunctions_NonNullableTheoryData> DateFunctions_NonNullable_Data
            =>
            [
                new DateFunctions_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new YearDescriptor
                        (
                            new MemberSelectorDescriptor("DateProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(2015)
                    ),
                    "$it => ($it.DateProperty.Year == 2015)"
                ),
                new DateFunctions_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MonthDescriptor
                        (
                            new MemberSelectorDescriptor("DateProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(12)
                    ),
                    "$it => ($it.DateProperty.Month == 12)"
                ),
                new DateFunctions_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new DayDescriptor
                        (
                            new MemberSelectorDescriptor("DateProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(23)
                    ),
                    "$it => ($it.DateProperty.Day == 23)"
                ),
            ];

        [Theory]
        [MemberData(nameof(DateFunctions_NonNullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void DateFunctions_NonNullable(DateFunctions_NonNullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class DateOnlyFunctions_NonNullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<DateOnlyFunctions_NonNullableTheoryData> DateOnlyFunctions_NonNullable_Data
            =>
            [
                new DateOnlyFunctions_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new YearDescriptor
                        (
                            new MemberSelectorDescriptor("DateOnlyProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(2015)
                    ),
                    "$it => ($it.DateOnlyProperty.Year == 2015)"
                ),
                new DateOnlyFunctions_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MonthDescriptor
                        (
                            new MemberSelectorDescriptor("DateOnlyProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(12)
                    ),
                    "$it => ($it.DateOnlyProperty.Month == 12)"
                ),
                new DateOnlyFunctions_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new DayDescriptor
                        (
                            new MemberSelectorDescriptor("DateOnlyProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(23)
                    ),
                    "$it => ($it.DateOnlyProperty.Day == 23)"
                ),
            ];

        [Theory]
        [MemberData(nameof(DateOnlyFunctions_NonNullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void DateOnlyFunctions_NonNullable(DateOnlyFunctions_NonNullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class TimeOfDayFunctions_NullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<TimeOfDayFunctions_NullableTheoryData> TimeOfDayFunctions_Nullable_Data
            =>
            [
                new TimeOfDayFunctions_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new HourDescriptor
                        (
                            new MemberSelectorDescriptor("NullableTimeOfDayProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(10)
                    ),
                    "$it => ($it.NullableTimeOfDayProperty.Value.Hours == 10)"
                ),
                new TimeOfDayFunctions_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MinuteDescriptor
                        (
                            new MemberSelectorDescriptor("NullableTimeOfDayProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(20)
                    ),
                    "$it => ($it.NullableTimeOfDayProperty.Value.Minutes == 20)"
                ),
                new TimeOfDayFunctions_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new SecondDescriptor
                        (
                            new MemberSelectorDescriptor("NullableTimeOfDayProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(30)
                    ),
                    "$it => ($it.NullableTimeOfDayProperty.Value.Seconds == 30)"
                ),
            ];

        [Theory]
        [MemberData(nameof(TimeOfDayFunctions_Nullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void TimeOfDayFunctions_Nullable(TimeOfDayFunctions_NullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class TimeOnlyFunctions_NullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<TimeOnlyFunctions_NullableTheoryData> TimeOnlyFunctions_Nullable_Data
            =>
            [
                new TimeOnlyFunctions_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new HourDescriptor
                        (
                            new MemberSelectorDescriptor("NullableTimeOnlyProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(10)
                    ),
                    "$it => ($it.NullableTimeOnlyProperty.Value.Hour == 10)"
                ),
                new TimeOnlyFunctions_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MinuteDescriptor
                        (
                            new MemberSelectorDescriptor("NullableTimeOnlyProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(20)
                    ),
                    "$it => ($it.NullableTimeOnlyProperty.Value.Minute == 20)"
                ),
                new TimeOnlyFunctions_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new SecondDescriptor
                        (
                            new MemberSelectorDescriptor("NullableTimeOnlyProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(30)
                    ),
                    "$it => ($it.NullableTimeOnlyProperty.Value.Second == 30)"
                ),
            ];

        [Theory]
        [MemberData(nameof(TimeOnlyFunctions_Nullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void TimeOnlyFunctions_Nullable(TimeOnlyFunctions_NullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class TimeOfDayFunctions_NonNullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<TimeOfDayFunctions_NonNullableTheoryData> TimeOfDayFunctions_NonNullable_Data
            =>
            [
                new TimeOfDayFunctions_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new HourDescriptor
                        (
                            new MemberSelectorDescriptor("TimeOfDayProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(10)
                    ),
                    "$it => ($it.TimeOfDayProperty.Hours == 10)"
                ),
                new TimeOfDayFunctions_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MinuteDescriptor
                        (
                            new MemberSelectorDescriptor("TimeOfDayProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(20)
                    ),
                    "$it => ($it.TimeOfDayProperty.Minutes == 20)"
                ),
                new TimeOfDayFunctions_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new SecondDescriptor
                        (
                            new MemberSelectorDescriptor("TimeOfDayProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(30)
                    ),
                    "$it => ($it.TimeOfDayProperty.Seconds == 30)"
                ),
            ];

        [Theory]
        [MemberData(nameof(TimeOfDayFunctions_NonNullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void TimeOfDayFunctions_NonNullable(TimeOfDayFunctions_NonNullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class TimeOnlyFunctions_NonNullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<TimeOnlyFunctions_NonNullableTheoryData> TimeOnlyFunctions_NonNullable_Data
            =>
            [
                new TimeOnlyFunctions_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new HourDescriptor
                        (
                            new MemberSelectorDescriptor("TimeOnlyProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(10)
                    ),
                    "$it => ($it.TimeOnlyProperty.Hour == 10)"
                ),
                new TimeOnlyFunctions_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MinuteDescriptor
                        (
                            new MemberSelectorDescriptor("TimeOnlyProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(20)
                    ),
                    "$it => ($it.TimeOnlyProperty.Minute == 20)"
                ),
                new TimeOnlyFunctions_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new SecondDescriptor
                        (
                            new MemberSelectorDescriptor("TimeOnlyProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(30)
                    ),
                    "$it => ($it.TimeOnlyProperty.Second == 30)"
                ),
            ];

        [Theory]
        [MemberData(nameof(TimeOnlyFunctions_NonNullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void TimeOnlyFunctions_NonNullable(TimeOnlyFunctions_NonNullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class FractionalsecondsFunction_NullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<FractionalsecondsFunction_NullableTheoryData> FractionalsecondsFunction_Nullable_Data
            =>
            [
                new FractionalsecondsFunction_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new FractionalSecondsDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(0.2m)
                    ),
                    "$it => ((Convert($it.DiscontinuedDate.Value.Millisecond) / 1000) == 0.2)"
                ),
                new FractionalsecondsFunction_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new FractionalSecondsDescriptor
                        (
                            new MemberSelectorDescriptor("NullableTimeOfDayProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(0.2m)
                    ),
                    "$it => ((Convert($it.NullableTimeOfDayProperty.Value.Milliseconds) / 1000) == 0.2)"
                ),
                new FractionalsecondsFunction_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new FractionalSecondsDescriptor
                        (
                            new MemberSelectorDescriptor("NullableTimeOnlyProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(0.2m)
                    ),
                    "$it => ((Convert($it.NullableTimeOnlyProperty.Value.Millisecond) / 1000) == 0.2)"
                ),
            ];

        [Theory]
        [MemberData(nameof(FractionalsecondsFunction_Nullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void FractionalsecondsFunction_Nullable(FractionalsecondsFunction_NullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class FractionalsecondsFunction_NonNullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<FractionalsecondsFunction_NonNullableTheoryData> FractionalsecondsFunction_NonNullable_Data
            =>
            [
                new FractionalsecondsFunction_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new FractionalSecondsDescriptor
                        (
                            new MemberSelectorDescriptor("NonNullableDiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(0.2m)
                    ),
                    "$it => ((Convert($it.NonNullableDiscontinuedDate.Millisecond) / 1000) == 0.2)"
                ),
                new FractionalsecondsFunction_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new FractionalSecondsDescriptor
                        (
                            new MemberSelectorDescriptor("TimeOfDayProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(0.2m)
                    ),
                    "$it => ((Convert($it.TimeOfDayProperty.Milliseconds) / 1000) == 0.2)"
                ),
                new FractionalsecondsFunction_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new FractionalSecondsDescriptor
                        (
                            new MemberSelectorDescriptor("TimeOnlyProperty", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(0.2m)
                    ),
                    "$it => ((Convert($it.TimeOnlyProperty.Millisecond) / 1000) == 0.2)"
                ),
            ];

        [Theory]
        [MemberData(nameof(FractionalsecondsFunction_NonNullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void FractionalsecondsFunction_NonNullable(FractionalsecondsFunction_NonNullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class DateFunction_NullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<DateFunction_NullableTheoryData> DateFunction_Nullable_Data
            =>
            [
                new DateFunction_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToNumericDateDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericDateDescriptor
                        (
                            new ConstantDescriptor(new Date(2015, 2, 26))
                        )
                    ),
                    "$it => (((($it.DiscontinuedDate.Value.Year * 10000) + ($it.DiscontinuedDate.Value.Month * 100)) + $it.DiscontinuedDate.Value.Day) == (((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day))"
                ),
                new DateFunction_NullableTheoryData
                (
                    new LessThanBinaryDescriptor
                    (
                        new ConvertToNumericDateDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericDateDescriptor
                        (
                            new ConstantDescriptor(new Date(2016, 2, 26))
                        )
                    ),
                    "$it => (((($it.DiscontinuedDate.Value.Year * 10000) + ($it.DiscontinuedDate.Value.Month * 100)) + $it.DiscontinuedDate.Value.Day) < (((2016-02-26.Year * 10000) + (2016-02-26.Month * 100)) + 2016-02-26.Day))"
                ),
                new DateFunction_NullableTheoryData
                (
                    new GreaterThanOrEqualsBinaryDescriptor
                    (
                        new ConvertToNumericDateDescriptor
                        (
                            new ConstantDescriptor(new Date(2015, 2, 26))
                        ),
                        new ConvertToNumericDateDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        )
                    ),
                    "$it => ((((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day) >= ((($it.DiscontinuedDate.Value.Year * 10000) + ($it.DiscontinuedDate.Value.Month * 100)) + $it.DiscontinuedDate.Value.Day))"
                ),
                new DateFunction_NullableTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(null),
                        new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                    ),
                    "$it => (null != $it.DiscontinuedDate)"
                ),
                new DateFunction_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(null)
                    ),
                    "$it => ($it.DiscontinuedDate == null)"
                ),
            ];

        [Theory]
        [MemberData(nameof(DateFunction_Nullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void DateFunction_Nullable(DateFunction_NullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class DateOnlyFunction_NullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<DateOnlyFunction_NullableTheoryData> DateOnlyFunction_Nullable_Data
            =>
            [
                new DateOnlyFunction_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToNumericDateDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericDateDescriptor
                        (
                            new ConstantDescriptor(new DateOnly(2015, 2, 26))
                        )
                    ),
                    "$it => (((($it.DiscontinuedDate.Value.Year * 10000) + ($it.DiscontinuedDate.Value.Month * 100)) + $it.DiscontinuedDate.Value.Day) == (((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day))"
                ),
                new DateOnlyFunction_NullableTheoryData
                (
                    new LessThanBinaryDescriptor
                    (
                        new ConvertToNumericDateDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericDateDescriptor
                        (
                            new ConstantDescriptor(new DateOnly(2016, 2, 26))
                        )
                    ),
                    "$it => (((($it.DiscontinuedDate.Value.Year * 10000) + ($it.DiscontinuedDate.Value.Month * 100)) + $it.DiscontinuedDate.Value.Day) < (((2016-02-26.Year * 10000) + (2016-02-26.Month * 100)) + 2016-02-26.Day))"
                ),
                new DateOnlyFunction_NullableTheoryData
                (
                    new GreaterThanOrEqualsBinaryDescriptor
                    (
                        new ConvertToNumericDateDescriptor
                        (
                            new ConstantDescriptor(new DateOnly(2015, 2, 26))
                        ),
                        new ConvertToNumericDateDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        )
                    ),
                    "$it => ((((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day) >= ((($it.DiscontinuedDate.Value.Year * 10000) + ($it.DiscontinuedDate.Value.Month * 100)) + $it.DiscontinuedDate.Value.Day))"
                ),
                new DateOnlyFunction_NullableTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(null),
                        new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                    ),
                    "$it => (null != $it.DiscontinuedDate)"
                ),
                new DateOnlyFunction_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(null)
                    ),
                    "$it => ($it.DiscontinuedDate == null)"
                ),
            ];

        [Theory]
        [MemberData(nameof(DateOnlyFunction_Nullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void DateOnlyFunction_Nullable(DateOnlyFunction_NullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class DateFunction_NonNullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<DateFunction_NonNullableTheoryData> DateFunction_NonNullable_Data
            =>
            [
                new DateFunction_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToNumericDateDescriptor
                        (
                            new MemberSelectorDescriptor("NonNullableDiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericDateDescriptor
                        (
                            new ConstantDescriptor(new Date(2015, 2, 26))
                        )
                    ),
                    "$it => (((($it.NonNullableDiscontinuedDate.Year * 10000) + ($it.NonNullableDiscontinuedDate.Month * 100)) + $it.NonNullableDiscontinuedDate.Day) == (((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day))"
                ),
                new DateFunction_NonNullableTheoryData
                (
                    new LessThanBinaryDescriptor
                    (
                        new ConvertToNumericDateDescriptor
                        (
                            new MemberSelectorDescriptor("NonNullableDiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericDateDescriptor
                        (
                            new ConstantDescriptor(new Date(2016, 2, 26))
                        )
                    ),
                    "$it => (((($it.NonNullableDiscontinuedDate.Year * 10000) + ($it.NonNullableDiscontinuedDate.Month * 100)) + $it.NonNullableDiscontinuedDate.Day) < (((2016-02-26.Year * 10000) + (2016-02-26.Month * 100)) + 2016-02-26.Day))"
                ),
                new DateFunction_NonNullableTheoryData
                (
                    new GreaterThanOrEqualsBinaryDescriptor
                    (
                        new ConvertToNumericDateDescriptor
                        (
                            new ConstantDescriptor(new Date(2015, 2, 26))
                        ),
                        new ConvertToNumericDateDescriptor
                        (
                            new MemberSelectorDescriptor("NonNullableDiscontinuedDate", new ParameterDescriptor(parameterName))
                        )
                    ),
                    "$it => ((((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day) >= ((($it.NonNullableDiscontinuedDate.Year * 10000) + ($it.NonNullableDiscontinuedDate.Month * 100)) + $it.NonNullableDiscontinuedDate.Day))"
                )
            ];

        [Theory]
        [MemberData(nameof(DateFunction_NonNullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void DateFunction_NonNullable(DateFunction_NonNullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class DateOnlyFunction_NonNullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<DateOnlyFunction_NonNullableTheoryData> DateOnlyFunction_NonNullable_Data
            =>
            [
                new DateOnlyFunction_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToNumericDateDescriptor
                        (
                            new MemberSelectorDescriptor("NonNullableDiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericDateDescriptor
                        (
                            new ConstantDescriptor(new DateOnly(2015, 2, 26))
                        )
                    ),
                    "$it => (((($it.NonNullableDiscontinuedDate.Year * 10000) + ($it.NonNullableDiscontinuedDate.Month * 100)) + $it.NonNullableDiscontinuedDate.Day) == (((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day))"
                ),
                new DateOnlyFunction_NonNullableTheoryData
                (
                    new LessThanBinaryDescriptor
                    (
                        new ConvertToNumericDateDescriptor
                        (
                            new MemberSelectorDescriptor("NonNullableDiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericDateDescriptor
                        (
                            new ConstantDescriptor(new DateOnly(2016, 2, 26))
                        )
                    ),
                    "$it => (((($it.NonNullableDiscontinuedDate.Year * 10000) + ($it.NonNullableDiscontinuedDate.Month * 100)) + $it.NonNullableDiscontinuedDate.Day) < (((2016-02-26.Year * 10000) + (2016-02-26.Month * 100)) + 2016-02-26.Day))"
                ),
                new DateOnlyFunction_NonNullableTheoryData
                (
                    new GreaterThanOrEqualsBinaryDescriptor
                    (
                        new ConvertToNumericDateDescriptor
                        (
                            new ConstantDescriptor(new DateOnly(2015, 2, 26))
                        ),
                        new ConvertToNumericDateDescriptor
                        (
                            new MemberSelectorDescriptor("NonNullableDiscontinuedDate", new ParameterDescriptor(parameterName))
                        )
                    ),
                    "$it => ((((2015-02-26.Year * 10000) + (2015-02-26.Month * 100)) + 2015-02-26.Day) >= ((($it.NonNullableDiscontinuedDate.Year * 10000) + ($it.NonNullableDiscontinuedDate.Month * 100)) + $it.NonNullableDiscontinuedDate.Day))"
                )
            ];

        [Theory]
        [MemberData(nameof(DateOnlyFunction_NonNullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void DateOnlyFunction_NonNullable(DateOnlyFunction_NonNullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class TimeFunction_NullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<TimeFunction_NullableTheoryData> TimeFunction_Nullable_Data
            =>
            [
                new TimeFunction_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToNumericTimeDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericTimeDescriptor
                        (
                            new ConstantDescriptor(new TimeOfDay(1, 2, 3, 4))
                        )
                    ),
                    "$it => (((Convert($it.DiscontinuedDate.Value.Hour) * 36000000000) + ((Convert($it.DiscontinuedDate.Value.Minute) * 600000000) + ((Convert($it.DiscontinuedDate.Value.Second) * 10000000) + Convert($it.DiscontinuedDate.Value.Millisecond)))) == ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))"
                ),
                new TimeFunction_NullableTheoryData
                (
                    new GreaterThanOrEqualsBinaryDescriptor
                    (
                        new ConvertToNumericTimeDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericTimeDescriptor
                        (
                            new ConstantDescriptor(new TimeOfDay(1, 2, 3, 4))
                        )
                    ),
                    "$it => (((Convert($it.DiscontinuedDate.Value.Hour) * 36000000000) + ((Convert($it.DiscontinuedDate.Value.Minute) * 600000000) + ((Convert($it.DiscontinuedDate.Value.Second) * 10000000) + Convert($it.DiscontinuedDate.Value.Millisecond)))) >= ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))"
                ),
                new TimeFunction_NullableTheoryData
                (
                    new LessThanOrEqualsBinaryDescriptor
                    (
                        new ConvertToNumericTimeDescriptor
                        (
                            new ConstantDescriptor(new TimeOfDay(1, 2, 3, 4))
                        ),
                        new ConvertToNumericTimeDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        )
                    ),
                    "$it => (((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))) <= ((Convert($it.DiscontinuedDate.Value.Hour) * 36000000000) + ((Convert($it.DiscontinuedDate.Value.Minute) * 600000000) + ((Convert($it.DiscontinuedDate.Value.Second) * 10000000) + Convert($it.DiscontinuedDate.Value.Millisecond)))))"
                ),
                new TimeFunction_NullableTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(null),
                        new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                    ),
                    "$it => (null != $it.DiscontinuedDate)"
                ),
                new TimeFunction_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(null)
                    ),
                    "$it => ($it.DiscontinuedDate == null)"
                )
            ];

        [Theory]
        [MemberData(nameof(TimeFunction_Nullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void TimeFunction_Nullable(TimeFunction_NullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class TimeOnlyFunction_NullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<TimeOnlyFunction_NullableTheoryData> TimeOnlyFunction_Nullable_Data
            =>
            [
                new TimeOnlyFunction_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToNumericTimeDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericTimeDescriptor
                        (
                            new ConstantDescriptor(new TimeOnly(1, 2, 3, 4))
                        )
                    ),
                    "$it => (((Convert($it.DiscontinuedDate.Value.Hour) * 36000000000) + ((Convert($it.DiscontinuedDate.Value.Minute) * 600000000) + ((Convert($it.DiscontinuedDate.Value.Second) * 10000000) + Convert($it.DiscontinuedDate.Value.Millisecond)))) == ((Convert(01:02:03.0040000.Hour) * 36000000000) + ((Convert(01:02:03.0040000.Minute) * 600000000) + ((Convert(01:02:03.0040000.Second) * 10000000) + Convert(01:02:03.0040000.Millisecond)))))"
                ),
                new TimeOnlyFunction_NullableTheoryData
                (
                    new GreaterThanOrEqualsBinaryDescriptor
                    (
                        new ConvertToNumericTimeDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericTimeDescriptor
                        (
                            new ConstantDescriptor(new TimeOnly(1, 2, 3, 4))
                        )
                    ),
                    "$it => (((Convert($it.DiscontinuedDate.Value.Hour) * 36000000000) + ((Convert($it.DiscontinuedDate.Value.Minute) * 600000000) + ((Convert($it.DiscontinuedDate.Value.Second) * 10000000) + Convert($it.DiscontinuedDate.Value.Millisecond)))) >= ((Convert(01:02:03.0040000.Hour) * 36000000000) + ((Convert(01:02:03.0040000.Minute) * 600000000) + ((Convert(01:02:03.0040000.Second) * 10000000) + Convert(01:02:03.0040000.Millisecond)))))"
                ),
                new TimeOnlyFunction_NullableTheoryData
                (
                    new LessThanOrEqualsBinaryDescriptor
                    (
                        new ConvertToNumericTimeDescriptor
                        (
                            new ConstantDescriptor(new TimeOnly(1, 2, 3, 4))
                        ),
                        new ConvertToNumericTimeDescriptor
                        (
                            new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                        )
                    ),
                    "$it => (((Convert(01:02:03.0040000.Hour) * 36000000000) + ((Convert(01:02:03.0040000.Minute) * 600000000) + ((Convert(01:02:03.0040000.Second) * 10000000) + Convert(01:02:03.0040000.Millisecond)))) <= ((Convert($it.DiscontinuedDate.Value.Hour) * 36000000000) + ((Convert($it.DiscontinuedDate.Value.Minute) * 600000000) + ((Convert($it.DiscontinuedDate.Value.Second) * 10000000) + Convert($it.DiscontinuedDate.Value.Millisecond)))))"
                ),
                new TimeOnlyFunction_NullableTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(null),
                        new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName))
                    ),
                    "$it => (null != $it.DiscontinuedDate)"
                ),
                new TimeOnlyFunction_NullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("DiscontinuedDate", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(null)
                    ),
                    "$it => ($it.DiscontinuedDate == null)"
                )
            ];

        [Theory]
        [MemberData(nameof(TimeOnlyFunction_Nullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void TimeOnlyFunction_Nullable(TimeOnlyFunction_NullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class TimeFunction_NonNullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<TimeFunction_NonNullableTheoryData> TimeFunction_NonNullable_Data
            =>
            [
                new TimeFunction_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToNumericTimeDescriptor
                        (
                            new MemberSelectorDescriptor("NonNullableDiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericTimeDescriptor
                        (
                            new ConstantDescriptor(new TimeOfDay(1, 2, 3, 4))
                        )
                    ),
                    "$it => (((Convert($it.NonNullableDiscontinuedDate.Hour) * 36000000000) + ((Convert($it.NonNullableDiscontinuedDate.Minute) * 600000000) + ((Convert($it.NonNullableDiscontinuedDate.Second) * 10000000) + Convert($it.NonNullableDiscontinuedDate.Millisecond)))) == ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))"
                ),
                new TimeFunction_NonNullableTheoryData
                (
                    new GreaterThanOrEqualsBinaryDescriptor
                    (
                        new ConvertToNumericTimeDescriptor
                        (
                            new MemberSelectorDescriptor("NonNullableDiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericTimeDescriptor
                        (
                            new ConstantDescriptor(new TimeOfDay(1, 2, 3, 4))
                        )
                    ),
                    "$it => (((Convert($it.NonNullableDiscontinuedDate.Hour) * 36000000000) + ((Convert($it.NonNullableDiscontinuedDate.Minute) * 600000000) + ((Convert($it.NonNullableDiscontinuedDate.Second) * 10000000) + Convert($it.NonNullableDiscontinuedDate.Millisecond)))) >= ((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))))"
                ),
                new TimeFunction_NonNullableTheoryData
                (
                    new LessThanOrEqualsBinaryDescriptor
                    (
                        new ConvertToNumericTimeDescriptor
                        (
                            new ConstantDescriptor(new TimeOfDay(1, 2, 3, 4))
                        ),
                        new ConvertToNumericTimeDescriptor
                        (
                            new MemberSelectorDescriptor("NonNullableDiscontinuedDate", new ParameterDescriptor(parameterName))
                        )
                    ),
                    "$it => (((Convert(01:02:03.0040000.Hours) * 36000000000) + ((Convert(01:02:03.0040000.Minutes) * 600000000) + ((Convert(01:02:03.0040000.Seconds) * 10000000) + Convert(01:02:03.0040000.Milliseconds)))) <= ((Convert($it.NonNullableDiscontinuedDate.Hour) * 36000000000) + ((Convert($it.NonNullableDiscontinuedDate.Minute) * 600000000) + ((Convert($it.NonNullableDiscontinuedDate.Second) * 10000000) + Convert($it.NonNullableDiscontinuedDate.Millisecond)))))"
                )
            ];

        [Theory]
        [MemberData(nameof(TimeFunction_NonNullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void TimeFunction_NonNullable(TimeFunction_NonNullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class TimeOnlyFunction_NonNullableTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<TimeOnlyFunction_NonNullableTheoryData> TimeOnlyFunction_NonNullable_Data
            =>
            [
                new TimeOnlyFunction_NonNullableTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToNumericTimeDescriptor
                        (
                            new MemberSelectorDescriptor("NonNullableDiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericTimeDescriptor
                        (
                            new ConstantDescriptor(new TimeOnly(1, 2, 3, 4))
                        )
                    ),
                    "$it => (((Convert($it.NonNullableDiscontinuedDate.Hour) * 36000000000) + ((Convert($it.NonNullableDiscontinuedDate.Minute) * 600000000) + ((Convert($it.NonNullableDiscontinuedDate.Second) * 10000000) + Convert($it.NonNullableDiscontinuedDate.Millisecond)))) == ((Convert(01:02:03.0040000.Hour) * 36000000000) + ((Convert(01:02:03.0040000.Minute) * 600000000) + ((Convert(01:02:03.0040000.Second) * 10000000) + Convert(01:02:03.0040000.Millisecond)))))"
                ),
                new TimeOnlyFunction_NonNullableTheoryData
                (
                    new GreaterThanOrEqualsBinaryDescriptor
                    (
                        new ConvertToNumericTimeDescriptor
                        (
                            new MemberSelectorDescriptor("NonNullableDiscontinuedDate", new ParameterDescriptor(parameterName))
                        ),
                        new ConvertToNumericTimeDescriptor
                        (
                            new ConstantDescriptor(new TimeOnly(1, 2, 3, 4))
                        )
                    ),
                    "$it => (((Convert($it.NonNullableDiscontinuedDate.Hour) * 36000000000) + ((Convert($it.NonNullableDiscontinuedDate.Minute) * 600000000) + ((Convert($it.NonNullableDiscontinuedDate.Second) * 10000000) + Convert($it.NonNullableDiscontinuedDate.Millisecond)))) >= ((Convert(01:02:03.0040000.Hour) * 36000000000) + ((Convert(01:02:03.0040000.Minute) * 600000000) + ((Convert(01:02:03.0040000.Second) * 10000000) + Convert(01:02:03.0040000.Millisecond)))))"
                ),
                new TimeOnlyFunction_NonNullableTheoryData
                (
                    new LessThanOrEqualsBinaryDescriptor
                    (
                        new ConvertToNumericTimeDescriptor
                        (
                            new ConstantDescriptor(new TimeOnly(1, 2, 3, 4))
                        ),
                        new ConvertToNumericTimeDescriptor
                        (
                            new MemberSelectorDescriptor("NonNullableDiscontinuedDate", new ParameterDescriptor(parameterName))
                        )
                    ),
                    "$it => (((Convert(01:02:03.0040000.Hour) * 36000000000) + ((Convert(01:02:03.0040000.Minute) * 600000000) + ((Convert(01:02:03.0040000.Second) * 10000000) + Convert(01:02:03.0040000.Millisecond)))) <= ((Convert($it.NonNullableDiscontinuedDate.Hour) * 36000000000) + ((Convert($it.NonNullableDiscontinuedDate.Minute) * 600000000) + ((Convert($it.NonNullableDiscontinuedDate.Second) * 10000000) + Convert($it.NonNullableDiscontinuedDate.Millisecond)))))"
                )
            ];

        [Theory]
        [MemberData(nameof(TimeOnlyFunction_NonNullable_Data), MemberType = typeof(FilterDescriptorTests))]
        public void TimeOnlyFunction_NonNullable(TimeOnlyFunction_NonNullableTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }
        #endregion Date Functions

        #region Math Functions
        [Fact]
        public void RecursiveMethodCall()
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { UnitPrice = 123.3m });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.UnitPrice.Value.Floor().Floor() == 123)");
            Assert.True(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new FloorDescriptor
                        (
                            new FloorDescriptor
                            (
                                new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName))
                            )
                        ),
                        new ConstantDescriptor(123m)
                    )
                );
        }

        [Fact]
        public void RecursiveMethodCallInvalidOperationException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.UnitPrice.Value.Floor().Floor() == 123)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new FloorDescriptor
                        (
                            new FloorDescriptor
                            (
                                new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName))
                            )
                        ),
                        new ConstantDescriptor(123m)
                    )
                );
        }

        [Fact]
        public void MathRoundDecimalInvalidOperationException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice.Value.Round() > {0:0.00})", 5.0));
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new RoundDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(5.00m)
                    )
                );
        }

        public class MathRoundDecimalTheoryData(decimal? unitPrice, bool expected)
        {
            public decimal? UnitPrice { get; } = unitPrice;
            public bool Expected { get; } = expected;
        }

        public static TheoryData<MathRoundDecimalTheoryData> MathRoundDecimal_DataSet
            =>
                [
                    new MathRoundDecimalTheoryData(5.9m, true),
                    new MathRoundDecimalTheoryData(5.4m, false)
                ];

        [Theory]
        [MemberData(nameof(MathRoundDecimal_DataSet), MemberType = typeof(FilterDescriptorTests))]
        public void MathRoundDecimal(MathRoundDecimalTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(theoryData.UnitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.UnitPrice.Value.Round() > {0:0.00})", 5.0));
            Assert.Equal(theoryData.Expected, result);
            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new RoundDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(5.00m)
                    )
                );
        }

        [Fact]
        public void MathRoundDoubleInvalidOperationException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.Weight.Value.Round() > {0})", 5));
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new RoundDescriptor
                        (
                            new MemberSelectorDescriptor("Weight", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(5d)
                    )
                );
        }

        [Theory]
        [InlineData(5.9d, true)]
        [InlineData(5.4d, false)]
        public void MathRoundDouble(double? weight, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { Weight = ToNullable<double>(weight) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => ($it.Weight.Value.Round() > {0})", 5));
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new RoundDescriptor
                        (
                            new MemberSelectorDescriptor("Weight", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(5d)
                    )
                );
        }

        [Fact]
        public void MathRoundFloatInvalidOperationException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (Convert($it.Width).Value.Round() > {0})", 5));
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new RoundDescriptor
                        (
                            new ConvertDescriptor(new MemberSelectorDescriptor("Width", new ParameterDescriptor(parameterName)), typeof(double?).AssemblyQualifiedName)
                        ),
                        new ConstantDescriptor(5d)
                    )
                );
        }

        [Theory]
        [InlineData(5.9f, true)]
        [InlineData(5.4f, false)]
        public void MathRoundFloat(float? width, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { Width = ToNullable<float>(width) });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, "$it => (Convert($it.Width).Value.Round() > {0})", 5));
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new RoundDescriptor
                        (
                            new ConvertDescriptor(new MemberSelectorDescriptor("Width", new ParameterDescriptor(parameterName)), typeof(double?).AssemblyQualifiedName)
                        ),
                        new ConstantDescriptor(5d)
                    )
                );
        }

        [Fact]
        public void MathFloorDecimalInvalidOperationException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.UnitPrice.Value.Floor() == 5)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new FloorDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(5m)
                    )
                );
        }

        public class MathFloorDecimalTheoryData(decimal? unitPrice, bool expected)
        {
            public decimal? UnitPrice { get; } = unitPrice;
            public bool Expected { get; } = expected;
        }

        public static TheoryData<MathFloorDecimalTheoryData> MathFloorDecimal_DataSet
            =>
                [
                    new MathFloorDecimalTheoryData(5.4m, true),
                    new MathFloorDecimalTheoryData(4.4m, false)
                ];

        [Theory, MemberData(nameof(MathFloorDecimal_DataSet), MemberType = typeof(FilterDescriptorTests))]
        public void MathFloorDecimal(MathFloorDecimalTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(theoryData.UnitPrice) });
            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.UnitPrice.Value.Floor() == 5)");
            Assert.Equal(theoryData.Expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new FloorDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(5m)
                    )
                );
        }

        [Fact]
        public void MathFloorDoubleInvalidOperationException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Weight.Value.Floor() == 5)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new FloorDescriptor
                        (
                            new MemberSelectorDescriptor("Weight", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(5d)
                    )
                );
        }

        [Theory]
        [InlineData(5.4d, true)]
        [InlineData(4.4d, false)]
        public void MathFloorDouble(double? weight, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { Weight = ToNullable<double>(weight) });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Weight.Value.Floor() == 5)");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new FloorDescriptor
                        (
                            new MemberSelectorDescriptor("Weight", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(5d)
                    )
                );
        }

        [Fact]
        public void MathFloorFloatInvalidOperationException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (Convert($it.Width).Value.Floor() == 5)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new FloorDescriptor
                        (
                            new ConvertDescriptor(new MemberSelectorDescriptor("Width", new ParameterDescriptor(parameterName)), typeof(double?).AssemblyQualifiedName)
                        ),
                        new ConstantDescriptor(5d)
                    )
                );
        }

        [Theory]
        [InlineData(5.4f, true)]
        [InlineData(4.4f, false)]
        public void MathFloorFloat(float? width, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { Width = ToNullable<float>(width) });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (Convert($it.Width).Value.Floor() == 5)");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new FloorDescriptor
                        (
                            new ConvertDescriptor(new MemberSelectorDescriptor("Width", new ParameterDescriptor(parameterName)), typeof(double?).AssemblyQualifiedName)
                        ),
                        new ConstantDescriptor(5d)
                    )
                );
        }

        [Fact]
        public void MathCeilingDecimalInvalidOperationException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.UnitPrice.Value.Ceiling() == 5)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new CeilingDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(5m)
                    )
                );
        }

        public class MathCeilingDecimalTheoryData(decimal? unitPrice, bool expected)
        {
            public decimal? UnitPrice { get; } = unitPrice;
            public bool Expected { get; } = expected;
        }

        public static TheoryData<MathCeilingDecimalTheoryData> MathCeilingDecimal_DataSet
            =>
                [
                    new MathCeilingDecimalTheoryData(4.1m, true),
                    new MathCeilingDecimalTheoryData(5.9m, false)
                ];

        [Theory, MemberData(nameof(MathCeilingDecimal_DataSet), MemberType = typeof(FilterDescriptorTests))]
        public void MathCeilingDecimal(MathCeilingDecimalTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { UnitPrice = ToNullable<decimal>(theoryData.UnitPrice) });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.UnitPrice.Value.Ceiling() == 5)");
            Assert.Equal(theoryData.Expected, result);
            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new CeilingDescriptor
                        (
                            new MemberSelectorDescriptor("UnitPrice", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(5m)
                    )
                );
        }

        [Fact]
        public void MathCeilingDoubleInvalidOperationException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Weight.Value.Ceiling() == 5)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new CeilingDescriptor
                        (
                            new MemberSelectorDescriptor("Weight", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(5d)
                    )
                );
        }

        [Theory]
        [InlineData(4.1d, true)]
        [InlineData(5.9d, false)]
        public void MathCeilingDouble(double? weight, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { Weight = ToNullable<double>(weight) });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.Weight.Value.Ceiling() == 5)");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new CeilingDescriptor
                        (
                            new MemberSelectorDescriptor("Weight", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor(5d)
                    )
                );
        }

        [Fact]
        public void MathCeilingFloatInvalidOperationException()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (Convert($it.Width).Value.Ceiling() == 5)");
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new Product { }));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new CeilingDescriptor
                        (
                            new ConvertDescriptor(new MemberSelectorDescriptor("Width", new ParameterDescriptor(parameterName)), typeof(double?).AssemblyQualifiedName)
                        ),
                        new ConstantDescriptor(5d)
                    )
                );
        }

        [Theory]
        [InlineData(4.1f, true)]
        [InlineData(5.9f, false)]
        public void MathCeilingFloat(float? width, bool expected)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { Width = ToNullable<float>(width) });

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (Convert($it.Width).Value.Ceiling() == 5)");
            Assert.Equal(expected, result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new CeilingDescriptor
                        (
                            new ConvertDescriptor(new MemberSelectorDescriptor("Width", new ParameterDescriptor(parameterName)), typeof(double?).AssemblyQualifiedName)
                        ),
                        new ConstantDescriptor(5d)
                    )
                );
        }

        public class MathFunctions_VariousTypesTheoryData(DescriptorBase filterBody)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
        }

        public static TheoryData<MathFunctions_VariousTypesTheoryData> MathFunctions_VariousTypes_Data
            =>
            [
                new MathFunctions_VariousTypesTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new FloorDescriptor
                        (
                            new ConvertDescriptor(new MemberSelectorDescriptor("FloatProp", new ParameterDescriptor(parameterName)), typeof(double).AssemblyQualifiedName)
                        ),
                        new FloorDescriptor
                        (
                            new ConvertDescriptor(new MemberSelectorDescriptor("FloatProp", new ParameterDescriptor(parameterName)), typeof(double).AssemblyQualifiedName)
                        )
                    )
                ),
                new MathFunctions_VariousTypesTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new RoundDescriptor
                        (
                            new ConvertDescriptor(new MemberSelectorDescriptor("FloatProp", new ParameterDescriptor(parameterName)), typeof(double).AssemblyQualifiedName)
                        ),
                        new RoundDescriptor
                        (
                            new ConvertDescriptor(new MemberSelectorDescriptor("FloatProp", new ParameterDescriptor(parameterName)), typeof(double).AssemblyQualifiedName)
                        )
                    )
                ),
                new MathFunctions_VariousTypesTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new CeilingDescriptor
                        (
                            new ConvertDescriptor(new MemberSelectorDescriptor("FloatProp", new ParameterDescriptor(parameterName)), typeof(double).AssemblyQualifiedName)
                        ),
                        new CeilingDescriptor
                        (
                            new ConvertDescriptor(new MemberSelectorDescriptor("FloatProp", new ParameterDescriptor(parameterName)), typeof(double).AssemblyQualifiedName)
                        )
                    )
                ),
                new MathFunctions_VariousTypesTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new FloorDescriptor
                        (
                            new MemberSelectorDescriptor("DoubleProp", new ParameterDescriptor(parameterName))
                        ),
                        new FloorDescriptor
                        (
                            new MemberSelectorDescriptor("DoubleProp", new ParameterDescriptor(parameterName))
                        )
                    )
                ),
                new MathFunctions_VariousTypesTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new RoundDescriptor
                        (
                            new MemberSelectorDescriptor("DoubleProp", new ParameterDescriptor(parameterName))
                        ),
                        new RoundDescriptor
                        (
                            new MemberSelectorDescriptor("DoubleProp", new ParameterDescriptor(parameterName))
                        )
                    )
                ),
                new MathFunctions_VariousTypesTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new CeilingDescriptor
                        (
                            new MemberSelectorDescriptor("DoubleProp", new ParameterDescriptor(parameterName))
                        ),
                        new CeilingDescriptor
                        (
                            new MemberSelectorDescriptor("DoubleProp", new ParameterDescriptor(parameterName))
                        )
                    )
                ),
                new MathFunctions_VariousTypesTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new FloorDescriptor
                        (
                            new MemberSelectorDescriptor("DecimalProp", new ParameterDescriptor(parameterName))
                        ),
                        new FloorDescriptor
                        (
                            new MemberSelectorDescriptor("DecimalProp", new ParameterDescriptor(parameterName))
                        )
                    )
                ),
                new MathFunctions_VariousTypesTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new RoundDescriptor
                        (
                            new MemberSelectorDescriptor("DecimalProp", new ParameterDescriptor(parameterName))
                        ),
                        new RoundDescriptor
                        (
                            new MemberSelectorDescriptor("DecimalProp", new ParameterDescriptor(parameterName))
                        )
                    )
                ),
                new MathFunctions_VariousTypesTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new CeilingDescriptor
                        (
                            new MemberSelectorDescriptor("DecimalProp", new ParameterDescriptor(parameterName))
                        ),
                        new CeilingDescriptor
                        (
                            new MemberSelectorDescriptor("DecimalProp", new ParameterDescriptor(parameterName))
                        )
                    )
                ),
            ];

        [Theory]
        [MemberData(nameof(MathFunctions_VariousTypes_Data), MemberType = typeof(FilterDescriptorTests))]
        public void MathFunctions_VariousTypes(MathFunctions_VariousTypesTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<DataTypes>();
            bool result = RunFilter(filter, new DataTypes { });

            //assert
            Assert.True(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }
        #endregion Math Functions

        #region Custom Functions
        [Fact]
        public void CustomMethod_InstanceMethodOfDeclaringType()
        {
            //arrange
            const string productName = "Abcd";
            const int totalWidth = 5;
            const string expectedProductName = "Abcd ";

            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            Assert.True(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new CustomMethodDescriptor
                        (
                            typeof(string).AssemblyQualifiedName,
                            "PadRight",
                            [typeof(int).AssemblyQualifiedName],
                            new DescriptorBase[]
                            {
                                new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                                new ConstantDescriptor(totalWidth)
                            }
                        ),
                        new ConstantDescriptor(expectedProductName)
                    )
                );
        }

        [Fact]
        public void CustomMethod_StaticExtensionMethod()
        {
            //arrange
            const string productName = "Abcd";
            const int totalWidth = 5;
            const string expectedProductName = "Abcd ";

            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            Assert.True(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new CustomMethodDescriptor
                        (
                            typeof(StringExtender).AssemblyQualifiedName,
                            "PadRightExStatic",
                            [typeof(string).AssemblyQualifiedName, typeof(int).AssemblyQualifiedName],
                            new DescriptorBase[]
                            {
                                new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                                new ConstantDescriptor(totalWidth)
                            }
                        ),
                        new ConstantDescriptor(expectedProductName)
                    )
                );
        }

        [Fact]
        public void CustomMethod_StaticMethoOfDeclaringType()
        {
            //arrange
            const string productName = "Abcd";
            const int totalWidth = 5;
            const string expectedProductName = "Abcd ";

            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = productName });

            //assert
            Assert.True(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new CustomMethodDescriptor
                        (
                            typeof(FilterDescriptorTests).AssemblyQualifiedName,
                            nameof(PadRightStatic),
                            [typeof(string).AssemblyQualifiedName, typeof(int).AssemblyQualifiedName],
                            new DescriptorBase[]
                            {
                                new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                                new ConstantDescriptor(totalWidth)
                            }
                        ),
                        new ConstantDescriptor(expectedProductName)
                    )
                );
        }
        #endregion Custom Functions

        #region Data Types
        [Fact]
        public void GuidExpression()
        {
            //act
            var filter1 = CreateFilter1<DataTypes>();
            var filter2 = CreateFilter2<DataTypes>();

            //assert
            AssertFilterStringIsCorrect(filter1, "$it => ($it.GuidProp == 0efdaecf-a9f0-42f3-a384-1295917af95e)");
            AssertFilterStringIsCorrect(filter2, "$it => ($it.GuidProp == 0efdaecf-a9f0-42f3-a384-1295917af95e)");

            Expression<Func<T, bool>> CreateFilter1<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("GuidProp", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(new Guid("0EFDAECF-A9F0-42F3-A384-1295917AF95E"))
                    )
                );

            Expression<Func<T, bool>> CreateFilter2<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("GuidProp", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(new Guid("0efdaecf-a9f0-42f3-a384-1295917af95e"))
                    )
                );
        }

        public class DateTimeExpressionTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<DateTimeExpressionTheoryData> DateTimeExpression_Data
            =>
            [
                new DateTimeExpressionTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("DateTimeProp", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(new DateTimeOffset(new DateTime(2000, 12, 12, 12, 0, 0, DateTimeKind.Unspecified), TimeSpan.Zero))
                    ),
                    "$it => ($it.DateTimeProp == {0})"
                ),
                new DateTimeExpressionTheoryData
                (
                    new LessThanBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("DateTimeProp", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(new DateTimeOffset(new DateTime(2000, 12, 12, 12, 0, 0, DateTimeKind.Unspecified), TimeSpan.Zero))
                    ),
                    "$it => ($it.DateTimeProp < {0})"
                )
            ];

        [Theory]
        [MemberData(nameof(DateTimeExpression_Data), MemberType = typeof(FilterDescriptorTests))]
        public void DateTimeExpression(DateTimeExpressionTheoryData theoryData)
        {
            //arrange
            var dateTime = new DateTimeOffset(new DateTime(2000, 12, 12, 12, 0, 0, DateTimeKind.Unspecified), TimeSpan.Zero);

            //act
            var filter = CreateFilter<DataTypes>();

            //assert
            AssertFilterStringIsCorrect(filter, string.Format(CultureInfo.InvariantCulture, theoryData.ExpectedExpression, dateTime));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        [Fact]
        public void IntegerLiteralSuffix()
        {
            //act
            var filter1 = CreateFilter1<DataTypes>();
            var filter2 = CreateFilter2<DataTypes>();

            //assert
            AssertFilterStringIsCorrect(filter1, "$it => (($it.LongProp < 987654321) AndAlso ($it.LongProp > 123456789))");
            AssertFilterStringIsCorrect(filter2, "$it => (($it.LongProp < -987654321) AndAlso ($it.LongProp > -123456789))");

            Expression<Func<T, bool>> CreateFilter1<T>()
                => GetFilter<T>
                (
                    new AndBinaryDescriptor
                    (
                        new LessThanBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("LongProp", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor((long)987654321, typeof(long).AssemblyQualifiedName)
                        ),
                        new GreaterThanBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("LongProp", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor((long)123456789, typeof(long).AssemblyQualifiedName)
                        )
                    )
                );

            Expression<Func<T, bool>> CreateFilter2<T>()
                => GetFilter<T>
                (
                    new AndBinaryDescriptor
                    (
                        new LessThanBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("LongProp", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor((long)-987654321, typeof(long).AssemblyQualifiedName)
                        ),
                        new GreaterThanBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("LongProp", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor((long)-123456789, typeof(long).AssemblyQualifiedName)
                        )
                    )
                );
        }

        [Fact]
        public void EnumInExpression()
        {
            //act
            var filter = CreateFilter<DataTypes>();
            var constant = (ConstantExpression)((MethodCallExpression)filter.Body).Arguments[0];
            var values = (IList<Position>)constant.Value;

            //assert
            AssertFilterStringIsCorrect(filter, "$it => System.Collections.Generic.List`1[LogicBuilder.EntityFrameworkCore.SqlServer.Tests.Data.Position].Contains($it.SimpleEnumProp)");
            Assert.Equal([Position.First, Position.Second], values);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new InDescriptor
                    (
                        new MemberSelectorDescriptor("SimpleEnumProp", new ParameterDescriptor(parameterName)),
                        new CollectionConstantDescriptor(new List<object> { Position.First, Position.Second }, typeof(Position).AssemblyQualifiedName)
                    )
                );
        }

        [Fact]
        public void EnumInExpression_NullableEnum_WithNullable()
        {
            //act
            var filter = CreateFilter<DataTypes>();
            var constant = (ConstantExpression)((MethodCallExpression)filter.Body).Arguments[0];
            var values = (IList<Position?>)constant.Value;

            //assert
            AssertFilterStringIsCorrect(filter, "$it => System.Collections.Generic.List`1[System.Nullable`1[LogicBuilder.EntityFrameworkCore.SqlServer.Tests.Data.Position]].Contains($it.NullableSimpleEnumProp)");
            Assert.Equal([Position.First, Position.Second], values);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new InDescriptor
                    (
                        new MemberSelectorDescriptor("NullableSimpleEnumProp", new ParameterDescriptor(parameterName)),
                        new CollectionConstantDescriptor(new List<object> { Position.First, Position.Second }, typeof(Position?).AssemblyQualifiedName)
                    )
                );
        }

        [Fact]
        public void EnumInExpression_NullableEnum_WithNullValue()
        {
            //act
            var filter = CreateFilter<DataTypes>();
            var constant = (ConstantExpression)((MethodCallExpression)filter.Body).Arguments[0];
            var values = (IList<Position?>)constant.Value;

            //assert
            AssertFilterStringIsCorrect(filter, "$it => System.Collections.Generic.List`1[System.Nullable`1[LogicBuilder.EntityFrameworkCore.SqlServer.Tests.Data.Position]].Contains($it.NullableSimpleEnumProp)");
            Assert.Equal([Position.First, null], values);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new InDescriptor
                    (
                        new MemberSelectorDescriptor("NullableSimpleEnumProp", new ParameterDescriptor(parameterName)),
                        new CollectionConstantDescriptor(new List<object> { Position.First, null }, typeof(Position?).AssemblyQualifiedName)
                    )
                );
        }

        [Fact]
        public void RealLiteralSuffixes()
        {
            //act
            var filter1 = CreateFilter1<DataTypes>();
            var filter2 = CreateFilter2<DataTypes>();

            //assert
            AssertFilterStringIsCorrect(filter1, string.Format(CultureInfo.InvariantCulture, "$it => (($it.FloatProp < {0:0.00}) AndAlso ($it.FloatProp > {1:0.00}))", 4321.56, 1234.56));
            AssertFilterStringIsCorrect(filter2, string.Format(CultureInfo.InvariantCulture, "$it => (($it.DecimalProp < {0:0.00}) AndAlso ($it.DecimalProp > {1:0.00}))", 4321.56, 1234.56));

            Expression<Func<T, bool>> CreateFilter1<T>()
                => GetFilter<T>
                (
                    new AndBinaryDescriptor
                    (
                        new LessThanBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("FloatProp", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(4321.56F)
                        ),
                        new GreaterThanBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("FloatProp", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(1234.56f)
                        )
                    )
                );

            Expression<Func<T, bool>> CreateFilter2<T>()
                => GetFilter<T>
                (
                    new AndBinaryDescriptor
                    (
                        new LessThanBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("DecimalProp", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(4321.56M)
                        ),
                        new GreaterThanBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("DecimalProp", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor(1234.56m)
                        )
                    )
                );
        }

        [Theory]
        [InlineData("hello,world", "hello,world")]
        [InlineData("'hello,world", "'hello,world")]
        [InlineData("hello,world'", "hello,world'")]
        [InlineData("hello,'wor'ld", "hello,'wor'ld")]
        [InlineData("hello,''world", "hello,''world")]
        [InlineData("\"hello,world\"", "\"hello,world\"")]
        [InlineData("\"hello,world", "\"hello,world")]
        [InlineData("hello,world\"", "hello,world\"")]
        [InlineData("hello,\"world", "hello,\"world")]
        [InlineData("México D.F.", "México D.F.")]
        [InlineData("æææøøøååå", "æææøøøååå")]
        [InlineData("いくつかのテキスト", "いくつかのテキスト")]
        public void StringLiterals(string literal, string expected)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, string.Format("$it => ($it.ProductName == \"{0}\")", expected));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(literal)
                    )
                );
        }

        [Theory]
        [InlineData('$')]
        [InlineData('&')]
        [InlineData('+')]
        [InlineData(',')]
        [InlineData('/')]
        [InlineData(':')]
        [InlineData(';')]
        [InlineData('=')]
        [InlineData('?')]
        [InlineData('@')]
        [InlineData(' ')]
        [InlineData('<')]
        [InlineData('>')]
        [InlineData('#')]
        [InlineData('%')]
        [InlineData('{')]
        [InlineData('}')]
        [InlineData('|')]
        [InlineData('\\')]
        [InlineData('^')]
        [InlineData('~')]
        [InlineData('[')]
        [InlineData(']')]
        [InlineData('`')]
        public void SpecialCharactersInStringLiteral(char c)
        {
            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, new Product { ProductName = c.ToString() });

            //assert
            AssertFilterStringIsCorrect(filter, string.Format("$it => ($it.ProductName == \"{0}\")", c));
            Assert.True(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(c.ToString())
                    )
                );
        }
        #endregion Data Types

        #region Casts
        [Fact]
        public void NSCast_OnEnumerableEntityCollection_GeneratesExpression_WithOfTypeOnEnumerable()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.EnumerableProducts.Cast().Any(p => (p.ProductName == \"ProductName\"))");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AnyDescriptor
                    (
                        new CollectionCastDescriptor
                        (
                            new MemberSelectorDescriptor
                            (
                                "EnumerableProducts",
                                new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                            ),
                            typeof(DerivedProduct).AssemblyQualifiedName
                        ),
                        new EqualsBinaryDescriptor
                        (
                             new MemberSelectorDescriptor("ProductName", new ParameterDescriptor("p")),
                             new ConstantDescriptor("ProductName")
                        ),
                        "p"
                    )
                );
        }

        [Fact]
        public void NSCast_OnQueryableEntityCollection_GeneratesExpression_WithOfTypeOnQueryable()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => $it.Category.QueryableProducts.Cast().Any(p => (p.ProductName == \"ProductName\"))");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AnyDescriptor
                    (
                        new CollectionCastDescriptor
                        (
                            new MemberSelectorDescriptor
                            (
                                "QueryableProducts",
                                new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                            ),
                            typeof(DerivedProduct).AssemblyQualifiedName
                        ),
                        new EqualsBinaryDescriptor
                        (
                             new MemberSelectorDescriptor("ProductName", new ParameterDescriptor("p")),
                             new ConstantDescriptor("ProductName")
                        ),
                        "p"
                    )
                );
        }

        [Fact]
        public void NSCast_OnEntityCollection_CanAccessDerivedInstanceProperty()
        {
            //act
            var filter = CreateFilter<Product>();
            bool result1 = RunFilter(filter, new Product { Category = new Category { Products = [new DerivedProduct { DerivedProductName = "DerivedProductName" }] } });
            bool result2 = RunFilter(filter, new Product { Category = new Category { Products = [new DerivedProduct { DerivedProductName = "NotDerivedProductName" }] } });

            //assert
            Assert.True(result1);
            Assert.False(result2);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new AnyDescriptor
                    (
                        new CollectionCastDescriptor
                        (
                            new MemberSelectorDescriptor
                            (
                                "Products",
                                new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                            ),
                            typeof(DerivedProduct).AssemblyQualifiedName
                        ),
                        new EqualsBinaryDescriptor
                        (
                             new MemberSelectorDescriptor("DerivedProductName", new ParameterDescriptor("p")),
                             new ConstantDescriptor("DerivedProductName")
                        ),
                        "p"
                    )
                );
        }

        [Fact]
        public void NSCast_OnSingleEntity_GeneratesExpression_WithAsDescriptor()
        {
            //act
            var filter = CreateFilter<DerivedProduct>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (($it As Product).ProductName == \"ProductName\")");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "ProductName",
                            new CastDescriptor
                            (
                                new ParameterDescriptor(parameterName),
                                typeof(Product).AssemblyQualifiedName
                            )
                        ),
                        new ConstantDescriptor("ProductName")
                    )
                );
        }

        public class Inheritance_WithDerivedInstanceTheoryData(DescriptorBase filterBody)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
        }

        public static TheoryData<Inheritance_WithDerivedInstanceTheoryData> Inheritance_WithDerivedInstance_Data
            =>
            [
                new Inheritance_WithDerivedInstanceTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "ProductName",
                            new CastDescriptor
                            (
                                new ParameterDescriptor(parameterName),
                                typeof(Product).AssemblyQualifiedName
                            )
                        ),
                        new ConstantDescriptor("ProductName")
                    )
                ),
                new Inheritance_WithDerivedInstanceTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "DerivedProductName",
                            new CastDescriptor
                            (
                                new ParameterDescriptor(parameterName),
                                typeof(DerivedProduct).AssemblyQualifiedName
                            )
                        ),
                        new ConstantDescriptor("DerivedProductName")
                    )
                ),
                new Inheritance_WithDerivedInstanceTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "CategoryID",
                            new MemberSelectorDescriptor
                            (
                                "Category",
                                new CastDescriptor
                                (
                                    new ParameterDescriptor(parameterName),
                                    typeof(DerivedProduct).AssemblyQualifiedName
                                )
                            )
                        ),
                        new ConstantDescriptor(123)
                    )
                ),
                new Inheritance_WithDerivedInstanceTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "CategoryID",
                            new CastDescriptor
                            (
                                new MemberSelectorDescriptor
                                (
                                    "Category",
                                    new CastDescriptor
                                    (
                                        new ParameterDescriptor(parameterName),
                                        typeof(DerivedProduct).AssemblyQualifiedName
                                    )
                                ),
                                typeof(DerivedCategory).AssemblyQualifiedName
                            )
                        ),
                        new ConstantDescriptor(123)
                    )
                ),
            ];

        [Theory]
        [MemberData(nameof(Inheritance_WithDerivedInstance_Data), MemberType = typeof(FilterDescriptorTests))]
        public void Inheritance_WithDerivedInstance(Inheritance_WithDerivedInstanceTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<DerivedProduct>();
            bool result = RunFilter(filter, new DerivedProduct { Category = new DerivedCategory { CategoryID = 123 }, ProductName = "ProductName", DerivedProductName = "DerivedProductName" });

            //assert
            Assert.True(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class Inheritance_WithBaseInstanceTheoryData(DescriptorBase filterBody)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
        }

        public static TheoryData<Inheritance_WithBaseInstanceTheoryData> Inheritance_WithBaseInstance_Data
            =>
            [
                new Inheritance_WithBaseInstanceTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "DerivedProductName",
                            new CastDescriptor
                            (
                                new ParameterDescriptor(parameterName),
                                typeof(DerivedProduct).AssemblyQualifiedName
                            )
                        ),
                        new ConstantDescriptor("DerivedProductName")
                    )
                ),
                new Inheritance_WithBaseInstanceTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "CategoryID",
                            new MemberSelectorDescriptor
                            (
                                "Category",
                                new CastDescriptor
                                (
                                    new ParameterDescriptor(parameterName),
                                    typeof(DerivedProduct).AssemblyQualifiedName
                                )
                            )
                        ),
                        new ConstantDescriptor(123)
                    )
                ),
                new Inheritance_WithBaseInstanceTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor
                        (
                            "CategoryID",
                            new CastDescriptor
                            (
                                new MemberSelectorDescriptor
                                (
                                    "Category",
                                    new CastDescriptor
                                    (
                                        new ParameterDescriptor(parameterName),
                                        typeof(DerivedProduct).AssemblyQualifiedName
                                    )
                                ),
                                typeof(DerivedCategory).AssemblyQualifiedName
                            )
                        ),
                        new ConstantDescriptor(123)
                    )
                ),
            ];

        [Theory]
        [MemberData(nameof(Inheritance_WithBaseInstance_Data), MemberType = typeof(FilterDescriptorTests))]
        public void Inheritance_WithBaseInstance(Inheritance_WithBaseInstanceTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            Assert.Throws<NullReferenceException>(() => RunFilter(filter, new Product()));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class CastMethod_SucceedsTheoryData(DescriptorBase filterBody, string expectedResult)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedResult { get; } = expectedResult;
        }

        public static TheoryData<CastMethod_SucceedsTheoryData> CastMethod_Succeeds_Data
            =>
            [
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(null),
                        new ConstantDescriptor(null)
                    ),
                    "$it => (null == null)"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(null),
                        new ConstantDescriptor(123)
                    ),
                    "$it => (null == Convert(123))"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(null),
                        new ConstantDescriptor(123)
                    ),
                    "$it => (null != Convert(123))"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(null),
                        new ConstantDescriptor(true)
                    ),
                    "$it => (null != Convert(True))"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(null),
                        new ConstantDescriptor(1)
                    ),
                    "$it => (null != Convert(1))"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(null),
                        new ConstantDescriptor(Guid.Empty)
                    ),
                    "$it => (null == Convert(00000000-0000-0000-0000-000000000000))"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(null),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (null != \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(null),
                        new ConstantDescriptor(new DateTimeOffset(new DateTime(2001, 1, 1, 12, 0, 0, DateTimeKind.Unspecified), new TimeSpan(8, 0, 0)))
                    ),
                    "$it => (null == Convert(01/01/2001 12:00:00 +08:00))"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(null),
                        new ConstantDescriptor(new TimeSpan(7775999999000))
                    ),
                    "$it => (null == Convert(8.23:59:59.9999000))"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("IntProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => ($it.IntProp.ToString() == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("LongProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => ($it.LongProp.ToString() == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("SingleProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => ($it.SingleProp.ToString() == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("DoubleProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => ($it.DoubleProp.ToString() == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("DecimalProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => ($it.DecimalProp.ToString() == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("BoolProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => ($it.BoolProp.ToString() == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("ByteProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => ($it.ByteProp.ToString() == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("GuidProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => ($it.GuidProp.ToString() == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("StringProp", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor("123")
                    ),
                    "$it => ($it.StringProp == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("DateTimeOffsetProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => ($it.DateTimeOffsetProp.ToString() == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("TimeSpanProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => ($it.TimeSpanProp.ToString() == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("SimpleEnumProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (Convert($it.SimpleEnumProp).ToString() == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("FlagsEnumProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (Convert($it.FlagsEnumProp).ToString() == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("LongEnumProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (Convert($it.LongEnumProp).ToString() == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("NullableIntProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (IIF($it.NullableIntProp.HasValue, $it.NullableIntProp.Value.ToString(), null) == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("NullableLongProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (IIF($it.NullableLongProp.HasValue, $it.NullableLongProp.Value.ToString(), null) == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("NullableSingleProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (IIF($it.NullableSingleProp.HasValue, $it.NullableSingleProp.Value.ToString(), null) == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("NullableDoubleProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (IIF($it.NullableDoubleProp.HasValue, $it.NullableDoubleProp.Value.ToString(), null) == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("NullableDecimalProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (IIF($it.NullableDecimalProp.HasValue, $it.NullableDecimalProp.Value.ToString(), null) == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("NullableBoolProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (IIF($it.NullableBoolProp.HasValue, $it.NullableBoolProp.Value.ToString(), null) == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("NullableByteProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (IIF($it.NullableByteProp.HasValue, $it.NullableByteProp.Value.ToString(), null) == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("NullableGuidProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (IIF($it.NullableGuidProp.HasValue, $it.NullableGuidProp.Value.ToString(), null) == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("NullableDateTimeOffsetProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (IIF($it.NullableDateTimeOffsetProp.HasValue, $it.NullableDateTimeOffsetProp.Value.ToString(), null) == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("NullableTimeSpanProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (IIF($it.NullableTimeSpanProp.HasValue, $it.NullableTimeSpanProp.Value.ToString(), null) == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new MemberSelectorDescriptor("NullableSimpleEnumProp", new ParameterDescriptor(parameterName))
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (IIF($it.NullableSimpleEnumProp.HasValue, Convert($it.NullableSimpleEnumProp.Value).ToString(), null) == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertDescriptor
                        (
                            new MemberSelectorDescriptor("IntProp", new ParameterDescriptor(parameterName)),
                            typeof(long).AssemblyQualifiedName
                        ),
                        new ConstantDescriptor((long)123)
                    ),
                    "$it => (Convert($it.IntProp) == 123)"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertDescriptor
                        (
                            new MemberSelectorDescriptor("NullableLongProp", new ParameterDescriptor(parameterName)),
                            typeof(double).AssemblyQualifiedName
                        ),
                        new ConstantDescriptor(1.23d)
                    ),
                    "$it => (Convert($it.NullableLongProp) == 1.23)"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new ConvertDescriptor
                        (
                            new ConstantDescriptor(2147483647),
                            typeof(short).AssemblyQualifiedName
                        ),
                        new ConstantDescriptor(null)
                    ),
                    "$it => (Convert(Convert(2147483647)) != null)"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new ConstantDescriptor(Position.Second, typeof(Position).AssemblyQualifiedName)
                        ),
                        new ConstantDescriptor("1")
                    ),
                    "$it => (Convert(Second).ToString() == \"1\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new ConvertDescriptor
                            (
                                new ConvertDescriptor
                                (
                                    new MemberSelectorDescriptor("IntProp", new ParameterDescriptor(parameterName)),
                                    typeof(long).AssemblyQualifiedName
                                ),
                                typeof(short).AssemblyQualifiedName
                            )
                        ),
                        new ConstantDescriptor("123")
                    ),
                    "$it => (Convert(Convert($it.IntProp)).ToString() == \"123\")"
                ),
                new CastMethod_SucceedsTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new ConvertToEnumDescriptor
                        (
                            "123",
                            typeof(Position).AssemblyQualifiedName
                        ),
                        new ConstantDescriptor(null)
                    ),
                    "$it => (Convert(123) != null)"
                )
            ];

        [Theory]
        [MemberData(nameof(CastMethod_Succeeds_Data), MemberType = typeof(FilterDescriptorTests))]
        public void CastMethod_Succeeds(CastMethod_SucceedsTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<DataTypes>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedResult);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }
        #endregion Casts

        #region 'isof' in query option
        public class IsofMethod_SucceedsTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<IsofMethod_SucceedsTheoryData> IsofMethod_Succeeds_Data
            =>
            [
                new IsofMethod_SucceedsTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(short).AssemblyQualifiedName
                    ),
                    "$it => IIF(($it Is System.Int16), True, False)"
                ),
                new IsofMethod_SucceedsTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(Product).AssemblyQualifiedName
                    ),
                    "$it => IIF(($it Is LogicBuilder.EntityFrameworkCore.SqlServer.Tests.Data.Product), True, False)"
                ),
                new IsofMethod_SucceedsTheoryData
                (
                    new IsOfDescriptor
                    (
                        new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                        typeof(string).AssemblyQualifiedName
                    ),
                    "$it => IIF(($it.ProductName Is System.String), True, False)"
                ),
                new IsofMethod_SucceedsTheoryData
                (
                    new IsOfDescriptor
                    (
                        new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName)),
                        typeof(Category).AssemblyQualifiedName
                    ),
                    "$it => IIF(($it.Category Is LogicBuilder.EntityFrameworkCore.SqlServer.Tests.Data.Category), True, False)"
                ),
                new IsofMethod_SucceedsTheoryData
                (
                    new IsOfDescriptor
                    (
                        new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName)),
                        typeof(DerivedCategory).AssemblyQualifiedName
                    ),
                    "$it => IIF(($it.Category Is LogicBuilder.EntityFrameworkCore.SqlServer.Tests.Data.DerivedCategory), True, False)"
                ),
                new IsofMethod_SucceedsTheoryData
                (
                    new IsOfDescriptor
                    (
                        new MemberSelectorDescriptor("Ranking", new ParameterDescriptor(parameterName)),
                        typeof(Position).AssemblyQualifiedName
                    ),
                    "$it => IIF(($it.Ranking Is LogicBuilder.EntityFrameworkCore.SqlServer.Tests.Data.Position), True, False)"
                ),
            ];

        [Theory]
        [MemberData(nameof(IsofMethod_Succeeds_Data), MemberType = typeof(FilterDescriptorTests))]
        public void IsofMethod_Succeeds(IsofMethod_SucceedsTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class IsOfPrimitiveType_Succeeds_WithFalseTheoryData(DescriptorBase filterBody)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
        }

        public static TheoryData<IsOfPrimitiveType_Succeeds_WithFalseTheoryData> IsOfPrimitiveType_Succeeds_WithFalse_Data
            =>
            [
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(byte[]).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(bool).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(byte).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(DateTimeOffset).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(Decimal).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(double).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(TimeSpan).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(Guid).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(Int16).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(Int32).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(Int64).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(sbyte).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(Single).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(System.IO.Stream).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(string).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(Position).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(Bits).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new MemberSelectorDescriptor("ByteArrayProp", new ParameterDescriptor(parameterName)),
                        typeof(byte[]).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new MemberSelectorDescriptor("IntProp", new ParameterDescriptor(parameterName)),
                        typeof(Position).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new MemberSelectorDescriptor("NullableShortProp", new ParameterDescriptor(parameterName)),
                        typeof(short).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(byte[]).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(bool).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(byte).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(DateTimeOffset).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(Decimal).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(double).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(TimeSpan).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(Guid).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(Int16).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(Int32).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(Int64).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(sbyte).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(Single).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(System.IO.Stream).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(string).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(Position).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(Bits).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(23),
                        typeof(byte).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(23),
                        typeof(decimal).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(23),
                        typeof(double).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(23),
                        typeof(short).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(23),
                        typeof(long).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(23),
                        typeof(sbyte).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(23),
                        typeof(float).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor("hello"),
                        typeof(Stream).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(0),
                        typeof(Bits).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(0),
                        typeof(Position).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor("2001-01-01T12:00:00.000+08:00"),
                        typeof(DateTimeOffset).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor("00000000-0000-0000-0000-000000000000"),
                        typeof(Guid).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor("23"),
                        typeof(byte).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor("23"),
                        typeof(short).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor("23"),
                        typeof(int).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor("false"),
                        typeof(bool).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor("OData"),
                        typeof(byte[]).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor("PT12H'"),
                        typeof(TimeSpan).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(23),
                        typeof(string).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor("0"),
                        typeof(Bits).AssemblyQualifiedName
                    )
                ),
                new IsOfPrimitiveType_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor("0"),
                        typeof(Position).AssemblyQualifiedName
                    )
                )
            ];

        [Theory]
        [MemberData(nameof(IsOfPrimitiveType_Succeeds_WithFalse_Data), MemberType = typeof(FilterDescriptorTests))]
        public void IsOfPrimitiveType_Succeeds_WithFalse(IsOfPrimitiveType_Succeeds_WithFalseTheoryData theoryData)
        {
            //arrange
            var model = new DataTypes();

            //act
            var filter = CreateFilter<DataTypes>();
            bool result = RunFilter(filter, model);

            //assert
            Assert.False(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class IsOfQuotedNonPrimitiveTypeTheoryData(DescriptorBase filterBody)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
        }

        public static TheoryData<IsOfQuotedNonPrimitiveTypeTheoryData> IsOfQuotedNonPrimitiveType
            =>
            [
                new IsOfQuotedNonPrimitiveTypeTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ParameterDescriptor(parameterName),
                        typeof(DerivedProduct).AssemblyQualifiedName
                    )
                ),
                new IsOfQuotedNonPrimitiveTypeTheoryData
                (
                    new IsOfDescriptor
                    (
                        new MemberSelectorDescriptor("SupplierAddress", new ParameterDescriptor(parameterName)),
                        typeof(Address).AssemblyQualifiedName
                    )
                ),
                new IsOfQuotedNonPrimitiveTypeTheoryData
                (
                    new IsOfDescriptor
                    (
                        new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName)),
                        typeof(DerivedCategory).AssemblyQualifiedName
                    )
                )
            ];

        [Theory]
        [MemberData(nameof(IsOfQuotedNonPrimitiveType), MemberType = typeof(FilterDescriptorTests))]
        public void IsOfQuotedNonPrimitiveType_Succeeds(IsOfQuotedNonPrimitiveTypeTheoryData theoryData)
        {
            //arrange
            var model = new DerivedProduct
            {
                SupplierAddress = new Address { City = "Redmond", },
                Category = new DerivedCategory { DerivedCategoryName = "DerivedCategory" }
            };

            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, model);

            //assert
            Assert.True(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class IsOfQuotedNonPrimitiveTypeWithNull_Succeeds_WithFalseTheoryData(DescriptorBase filterBody)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
        }

        public static TheoryData<IsOfQuotedNonPrimitiveTypeWithNull_Succeeds_WithFalseTheoryData> IsOfQuotedNonPrimitiveTypeWithNull_Succeeds_WithFalse_Data
            =>
            [
                new IsOfQuotedNonPrimitiveTypeWithNull_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(Address).AssemblyQualifiedName
                    )
                ),
                new IsOfQuotedNonPrimitiveTypeWithNull_Succeeds_WithFalseTheoryData
                (
                    new IsOfDescriptor
                    (
                        new ConstantDescriptor(null),
                        typeof(DerivedCategory).AssemblyQualifiedName
                    )
                )
            ];

        [Theory]
        [MemberData(nameof(IsOfQuotedNonPrimitiveTypeWithNull_Succeeds_WithFalse_Data), MemberType = typeof(FilterDescriptorTests))]
        public void IsOfQuotedNonPrimitiveTypeWithNull_Succeeds_WithFalse(IsOfQuotedNonPrimitiveTypeWithNull_Succeeds_WithFalseTheoryData theoryData)
        {
            //arrange
            var model = new DerivedProduct
            {
                SupplierAddress = new Address { City = "Redmond", },
                Category = new DerivedCategory { DerivedCategoryName = "DerivedCategory" }
            };

            //act
            var filter = CreateFilter<Product>();
            bool result = RunFilter(filter, model);

            //assert
            Assert.False(result);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }
        #endregion 'isof' in query option

        #region
        public class ByteArrayComparisonsTheoryData(DescriptorBase filterBody, string expectedExpression, bool expectedResult)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
            public bool ExpectedResult { get; } = expectedResult;
        }

        public static TheoryData<ByteArrayComparisonsTheoryData> ByteArrayComparisons_Data
            =>
            [
                new ByteArrayComparisonsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("ByteArrayProp", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(Convert.FromBase64String("I6v/"))
                    ),
                    "$it => ($it.ByteArrayProp == System.Byte[])",
                    true
                ),
                new ByteArrayComparisonsTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("ByteArrayProp", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(Convert.FromBase64String("I6v/"))
                    ),
                    "$it => ($it.ByteArrayProp != System.Byte[])",
                    false
                ),
                new ByteArrayComparisonsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(Convert.FromBase64String("I6v/")),
                        new ConstantDescriptor(Convert.FromBase64String("I6v/"))
                    ),
                    "$it => (System.Byte[] == System.Byte[])",
                    true
                ),
                new ByteArrayComparisonsTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(Convert.FromBase64String("I6v/")),
                        new ConstantDescriptor(Convert.FromBase64String("I6v/"))
                    ),
                    "$it => (System.Byte[] != System.Byte[])",
                    false
                ),
                new ByteArrayComparisonsTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("ByteArrayPropWithNullValue", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(Convert.FromBase64String("I6v/"))
                    ),
                    "$it => ($it.ByteArrayPropWithNullValue != System.Byte[])",
                    true
                ),
                new ByteArrayComparisonsTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("ByteArrayPropWithNullValue", new ParameterDescriptor(parameterName)),
                        new MemberSelectorDescriptor("ByteArrayPropWithNullValue", new ParameterDescriptor(parameterName))
                    ),
                    "$it => ($it.ByteArrayPropWithNullValue != $it.ByteArrayPropWithNullValue)",
                    false
                ),
                new ByteArrayComparisonsTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("ByteArrayPropWithNullValue", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(null)
                    ),
                    "$it => ($it.ByteArrayPropWithNullValue != null)",
                    false
                ),
                new ByteArrayComparisonsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("ByteArrayPropWithNullValue", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor(null)
                    ),
                    "$it => ($it.ByteArrayPropWithNullValue == null)",
                    true
                ),
                new ByteArrayComparisonsTheoryData
                (
                    new NotEqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(null),
                        new MemberSelectorDescriptor("ByteArrayPropWithNullValue", new ParameterDescriptor(parameterName))
                    ),
                    "$it => (null != $it.ByteArrayPropWithNullValue)",
                    false
                ),
                new ByteArrayComparisonsTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(null),
                        new MemberSelectorDescriptor("ByteArrayPropWithNullValue", new ParameterDescriptor(parameterName))
                    ),
                    "$it => (null == $it.ByteArrayPropWithNullValue)",
                    true
                ),
            ];

        [Theory]
        [MemberData(nameof(ByteArrayComparisons_Data), MemberType = typeof(FilterDescriptorTests))]
        public void ByteArrayComparisons(ByteArrayComparisonsTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<DataTypes>();
            bool result = RunFilter
            (
                filter,
                new DataTypes
                {
                    ByteArrayProp = [35, 171, 255]
                }
            );

            //assert
            Assert.Equal(theoryData.ExpectedResult, result);
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class DisAllowed_ByteArrayComparisonsTheoryData(DescriptorBase filterBody)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
        }

        public static TheoryData<DisAllowed_ByteArrayComparisonsTheoryData> DisAllowed_ByteArrayComparisons_Data
            =>
            [
                new DisAllowed_ByteArrayComparisonsTheoryData
                (
                    new GreaterThanOrEqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(Convert.FromBase64String("AP8Q")),
                        new ConstantDescriptor(Convert.FromBase64String("AP8Q"))
                    )
                ),
                new DisAllowed_ByteArrayComparisonsTheoryData
                (
                    new LessThanOrEqualsBinaryDescriptor
                    (
                        new ConstantDescriptor(Convert.FromBase64String("AP8Q")),
                        new ConstantDescriptor(Convert.FromBase64String("AP8Q"))
                    )
                ),
                new DisAllowed_ByteArrayComparisonsTheoryData
                (
                    new LessThanBinaryDescriptor
                    (
                        new ConstantDescriptor(Convert.FromBase64String("AP8Q")),
                        new ConstantDescriptor(Convert.FromBase64String("AP8Q"))
                    )
                ),
                new DisAllowed_ByteArrayComparisonsTheoryData
                (
                    new GreaterThanBinaryDescriptor
                    (
                        new ConstantDescriptor(Convert.FromBase64String("AP8Q")),
                        new ConstantDescriptor(Convert.FromBase64String("AP8Q"))
                    )
                ),
            ];

        [Theory]
        [MemberData(nameof(DisAllowed_ByteArrayComparisons_Data), MemberType = typeof(FilterDescriptorTests))]
        public void DisAllowed_ByteArrayComparisons(DisAllowed_ByteArrayComparisonsTheoryData theoryData)
        {
            //assert
            Assert.Throws<InvalidOperationException>(CreateFilter<DataTypes>);
            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class Nullable_NonstandardEdmPrimitivesTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<Nullable_NonstandardEdmPrimitivesTheoryData> Nullable_NonstandardEdmPrimitives_Data
            =>
            [
                new Nullable_NonstandardEdmPrimitivesTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertDescriptor
                        (
                            new ConvertToNullableUnderlyingValueDescriptor
                            (
                                new MemberSelectorDescriptor("NullableUShortProp", new ParameterDescriptor(parameterName))
                            ),
                            typeof(int?).AssemblyQualifiedName
                        ),
                        new ConstantDescriptor(12)
                    ),
                    "$it => (Convert($it.NullableUShortProp.Value) == Convert(12))"
                ),
                new Nullable_NonstandardEdmPrimitivesTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertDescriptor
                        (
                            new ConvertToNullableUnderlyingValueDescriptor
                            (
                                new MemberSelectorDescriptor("NullableULongProp", new ParameterDescriptor(parameterName))
                            ),
                            typeof(long?).AssemblyQualifiedName
                        ),
                        new ConstantDescriptor(12L)
                    ),
                    "$it => (Convert($it.NullableULongProp.Value) == Convert(12))"
                ),
                new Nullable_NonstandardEdmPrimitivesTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertDescriptor
                        (
                            new ConvertToNullableUnderlyingValueDescriptor
                            (
                                new MemberSelectorDescriptor("NullableUIntProp", new ParameterDescriptor(parameterName))
                            ),
                            typeof(int?).AssemblyQualifiedName
                        ),
                        new ConstantDescriptor(12)
                    ),
                    "$it => (Convert($it.NullableUIntProp.Value) == Convert(12))"
                ),
                new Nullable_NonstandardEdmPrimitivesTheoryData
                (
                    new EqualsBinaryDescriptor
                    (
                        new ConvertToStringDescriptor
                        (
                            new ConvertToNullableUnderlyingValueDescriptor
                            (
                                new MemberSelectorDescriptor("NullableCharProp", new ParameterDescriptor(parameterName))
                            )
                        ),
                        new ConstantDescriptor("a")
                    ),
                    "$it => ($it.NullableCharProp.Value.ToString() == \"a\")"
                ),
            ];

        [Theory]
        [MemberData(nameof(Nullable_NonstandardEdmPrimitives_Data), MemberType = typeof(FilterDescriptorTests))]
        public void Nullable_NonstandardEdmPrimitives(Nullable_NonstandardEdmPrimitivesTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<DataTypes>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);
            Assert.Throws<InvalidOperationException>(() => RunFilter(filter, new DataTypes()));

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        public class InOnNavigationTheoryData(DescriptorBase filterBody, string expectedExpression)
        {
            public DescriptorBase FilterBody { get; } = filterBody;
            public string ExpectedExpression { get; } = expectedExpression;
        }

        public static TheoryData<InOnNavigationTheoryData> InOnNavigation_Data
            =>
                [
                    new InOnNavigationTheoryData
                    (
                        new InDescriptor
                        (
                            new MemberSelectorDescriptor
                            (
                                "ProductID",
                                new MemberSelectorDescriptor
                                (
                                    "Product",
                                    new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                                )
                            ),
                            new CollectionConstantDescriptor
                            (
                                [1],
                                typeof(int).AssemblyQualifiedName
                            )
                        ),
                        "$it => System.Collections.Generic.List`1[System.Int32].Contains($it.Category.Product.ProductID)"
                    ),
                    new InOnNavigationTheoryData
                    (
                        new InDescriptor
                        (
                            new MemberSelectorDescriptor("Category.Product.ProductID", new ParameterDescriptor(parameterName)),
                            new CollectionConstantDescriptor
                            (
                                [1],
                                typeof(int).AssemblyQualifiedName
                            )
                        ),
                        "$it => System.Collections.Generic.List`1[System.Int32].Contains($it.Category.Product.ProductID)"
                    ),
                    new InOnNavigationTheoryData
                    (
                        new InDescriptor
                        (
                            new MemberSelectorDescriptor
                            (
                                "GuidProperty",
                                new MemberSelectorDescriptor
                                (
                                    "Product",
                                    new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                                )
                            ),
                            new CollectionConstantDescriptor
                            (
                                [new Guid("dc75698b-581d-488b-9638-3e28dd51d8f7")],
                                typeof(Guid).AssemblyQualifiedName
                            )
                        ),
                        "$it => System.Collections.Generic.List`1[System.Guid].Contains($it.Category.Product.GuidProperty)"
                    ),
                    new InOnNavigationTheoryData
                    (
                        new InDescriptor
                        (
                            new MemberSelectorDescriptor
                            (
                                "NullableGuidProperty",
                                new MemberSelectorDescriptor
                                (
                                    "Product",
                                    new MemberSelectorDescriptor("Category", new ParameterDescriptor(parameterName))
                                )
                            ),
                            new CollectionConstantDescriptor
                            (
                                [new Guid("dc75698b-581d-488b-9638-3e28dd51d8f7")],
                                typeof(Guid?).AssemblyQualifiedName
                            )
                        ),
                        "$it => System.Collections.Generic.List`1[System.Nullable`1[System.Guid]].Contains($it.Category.Product.NullableGuidProperty)"
                    )
                ];

        [Theory]
        [MemberData(nameof(InOnNavigation_Data), MemberType = typeof(FilterDescriptorTests))]
        public void InOnNavigation(InOnNavigationTheoryData theoryData)
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, theoryData.ExpectedExpression);

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    theoryData.FilterBody
                );
        }

        [Fact]
        public void MultipleConstants_Are_Parameterized()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => (((($it.ProductName == \"1\") OrElse ($it.ProductName == \"2\")) OrElse ($it.ProductName == \"3\")) OrElse ($it.ProductName == \"4\"))");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new OrBinaryDescriptor
                    (
                        new OrBinaryDescriptor
                        (
                            new OrBinaryDescriptor
                            (
                                new EqualsBinaryDescriptor
                                (
                                    new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                                    new ConstantDescriptor("1")
                                ),
                                new EqualsBinaryDescriptor
                                (
                                    new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                                    new ConstantDescriptor("2")
                                )
                            ),
                            new EqualsBinaryDescriptor
                            (
                                new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                                new ConstantDescriptor("3")
                            )
                        ),
                        new EqualsBinaryDescriptor
                        (
                            new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                            new ConstantDescriptor("4")
                        )
                    )
                );
        }

        [Fact]
        public void Constants_Are_Not_Parameterized_IfDisabled()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => ($it.ProductName == \"1\")");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new EqualsBinaryDescriptor
                    (
                        new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                        new ConstantDescriptor("1")
                    )
                );
        }

        [Fact]
        public void CollectionConstants_Are_Parameterized()
        {
            //act
            var filter = CreateFilter<Product>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => System.Collections.Generic.List`1[System.String].Contains($it.ProductName)");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new InDescriptor
                    (
                        new MemberSelectorDescriptor("ProductName", new ParameterDescriptor(parameterName)),
                        new CollectionConstantDescriptor
                        (
                            new List<object> { "Prod1", "Prod2" },
                            typeof(string).AssemblyQualifiedName
                        )
                    )
                );
        }

        [Fact]
        public void CollectionConstants_OfEnums_Are_Not_Parameterized_If_Disabled()
        {
            //act
            var filter = CreateFilter<DataTypes>();

            //assert
            AssertFilterStringIsCorrect(filter, "$it => System.Collections.Generic.List`1[LogicBuilder.EntityFrameworkCore.SqlServer.Tests.Data.Position].Contains($it.SimpleEnumProp)");

            Expression<Func<T, bool>> CreateFilter<T>()
                => GetFilter<T>
                (
                    new InDescriptor
                    (
                        new MemberSelectorDescriptor("SimpleEnumProp", new ParameterDescriptor(parameterName)),
                        new CollectionConstantDescriptor
                        (
                            new List<object> { Position.First, Position.Second },
                            typeof(Position).AssemblyQualifiedName
                        )
                    )
                );
        }
        #endregion

        private static void InitializeMapperConfiguration()
        {
            MapperConfiguration ??= ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();
                cfg.AddProfile<ExpressionOperatorsMappingProfile>();
            });
        }

        static MapperConfiguration MapperConfiguration;
        private void Initialize()
        {
            serviceProvider = new ServiceCollection()
                .AddSingleton<AutoMapper.IConfigurationProvider>
                (
                    MapperConfiguration
                )
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<AutoMapper.IConfigurationProvider>(), sp.GetService))
                .BuildServiceProvider();
        }

        // Used by Custom Method binder tests - by reflection
        private static string PadRightStatic(string str, int number)
        {
            return str.PadRight(number);
        }

        private static T? ToNullable<T>(object value) where T : struct =>
            value == null ? null : (T?)Convert.ChangeType(value, typeof(T));

        private static Dictionary<string, ParameterExpression> GetParameters()
            => [];

        private static void AssertFilterStringIsCorrect(Expression expression, string expected)
        {
            string resultExpression = ExpressionStringBuilder.ToString(expression);
            Assert.True(expected == resultExpression, string.Format("Expected expression '{0}' but the deserializer produced '{1}'", expected, resultExpression));
        }

        private Expression<Func<T, bool>> GetFilter<T>(DescriptorBase filterBody)
        {
            IMapper mapper = serviceProvider.GetRequiredService<IMapper>();

            return (Expression<Func<T, bool>>)mapper.Map<FilterLambdaOperator>
            (
                new FilterLambdaDescriptor
                (
                    filterBody,
                    typeof(T).AssemblyQualifiedName,
                    parameterName
                ),
                opts => opts.Items["parameters"] = GetParameters()
            ).Build();
        }

        private static bool RunFilter<TModel>(Expression<Func<TModel, bool>> filter, TModel instance)
            => filter.Compile().Invoke(instance);
    }

    public static class StringExtender
    {
        public static string PadRightExStatic(this string str, int width)
        {
            return str.PadRight(width);
        }
    }
}

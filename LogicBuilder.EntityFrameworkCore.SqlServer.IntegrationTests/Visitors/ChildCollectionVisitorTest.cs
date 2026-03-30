using AutoMapper;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models;
using LogicBuilder.EntityFrameworkCore.SqlServer.Visitors;
using LogicBuilder.Expressions.Utils.ExpressionBuilder;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Lambda;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Logical;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Operand;
using LogicBuilder.Expressions.Utils.Strutures;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Visitors
{
    public class ChildCollectionVisitorTest
    {
        [Fact]
        public void UpdaterExpansion_ShouldThrowNotSupportedException_WhenMultiplePartsOfTheExpressionMatchTheFilter()
        {
            // Arrange
            var parameters = new Dictionary<string, ParameterExpression>();
            var expansions = new List<Expressions.Utils.Expansions.ExpansionOptions>
            {
                new
                (
                    "Products",
                    typeof(ICollection<ProductModel>),
                    typeof(CategoryModel),
                    [], 
                    null, 
                    new Expressions.Utils.Expansions.ExpansionFilterOption
                    (
                        new FilterLambdaOperator
                        (
                            parameters,
                            new EqualsBinaryOperator
                            (
                                new MemberSelectorOperator("ProductName", new ParameterOperator(parameters, "a")),
                                new ConstantOperator("ProductOne")
                            ),
                            typeof(ProductModel),
                            "a"
                        )
                    )
                )
            };

            IQueryable<ProductModel> queryable = Enumerable.Empty<Product>().AsQueryable().Select(p => new ProductModel
            {
                Category = new CategoryModel
                {
                    Products = p.Category.Products.Select(i0 => new ProductModel
                    {
                        AlternateAddresses = i0.AlternateAddresses.Select(i1 => new AlternateAddressModel
                        {
                            Product = new ProductModel()
                        }).ToList()
                    }).ToList()
                },
                AlternateAddresses = p.AlternateAddresses.Select(i0 => new AlternateAddressModel
                {
                    Product = new ProductModel
                    {
                        Category = new CategoryModel
                        {
                            CategoryID = new int(),
                            Products = p.Category.Products.Select(i0 => new ProductModel
                            {
                                AlternateAddresses = i0.AlternateAddresses.Select(i1 => new AlternateAddressModel
                                {
                                    Product = new ProductModel()
                                }).ToList()
                            }).ToList()
                        },
                        AlternateAddresses = i0.Product.AlternateAddresses.Select(i1 => new AlternateAddressModel
                        {
                            Product = new ProductModel()
                        }).ToList()
                    }
                }).ToList()
            }).AsQueryable();
            var mapper = new Mapper(new MapperConfiguration(cfg => { }, NullLoggerFactory.Instance));

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => FilterUpdater.UpdaterExpansion(queryable.Expression, expansions, mapper));
        }

        [Fact]
        public void UpdaterExpansion_ShouldThrowNotSupportedException_WhenMultiplePartsOfTheExpressionMatchTheSort()
        {
            // Arrange
            var expansions = new List<Expressions.Utils.Expansions.ExpansionOptions>
            {
                new
                (
                    "Products",
                    typeof(ICollection<ProductModel>),
                    typeof(CategoryModel),
                    [],
                    new Expressions.Utils.Expansions.ExpansionQueryOption
                    (
                        new SortCollection
                        (
                            [
                                new SortDescription
                                (
                                    "ProductName",
                                    ListSortDirection.Ascending
                                )
                            ]
                        )
                    ),
                    null
                )
            };

            IQueryable<ProductModel> queryable = Enumerable.Empty<Product>().AsQueryable().Select(p => new ProductModel
            {
                Category = new CategoryModel
                {
                    Products = p.Category.Products.Select(i0 => new ProductModel
                    {
                        AlternateAddresses = i0.AlternateAddresses.Select(i1 => new AlternateAddressModel
                        {
                            Product = new ProductModel()
                        }).ToList()
                    }).ToList()
                },
                AlternateAddresses = p.AlternateAddresses.Select(i0 => new AlternateAddressModel
                {
                    Product = new ProductModel
                    {
                        Category = new CategoryModel
                        {
                            CategoryID = new int(),
                            Products = p.Category.Products.Select(i0 => new ProductModel
                            {
                                AlternateAddresses = i0.AlternateAddresses.Select(i1 => new AlternateAddressModel
                                {
                                    Product = new ProductModel()
                                }).ToList()
                            }).ToList()
                        },
                        AlternateAddresses = i0.Product.AlternateAddresses.Select(i1 => new AlternateAddressModel
                        {
                            Product = new ProductModel()
                        }).ToList()
                    }
                }).ToList()
            }).AsQueryable();

            // Act & Assert
            Assert.Throws<NotSupportedException>(() => QueryFunctionUpdater.UpdaterExpansion(queryable.Expression, expansions));
        }
    }
}

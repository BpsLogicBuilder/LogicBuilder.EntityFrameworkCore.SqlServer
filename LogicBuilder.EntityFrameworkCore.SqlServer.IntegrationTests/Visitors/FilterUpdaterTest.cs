using AutoMapper;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models;
using LogicBuilder.EntityFrameworkCore.SqlServer.Visitors;
using LogicBuilder.Expressions.Utils.Expansions;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Visitors
{
    public class FilterUpdaterTest
    {
        [Fact]
        public void UpdaterExpansion_ShouldThrowInvalidOperationException_WhenLastExpansionDoesNotHaveFilter()
        {
            // Arrange
            var expansions = new List<ExpansionOptions>
            {
                new("AlternateAddresses", typeof(ICollection<AlternateAddressModel>), typeof(ProductModel), [], null, null),
                new("Product", typeof(ProductModel), typeof(AlternateAddressModel), [], null, null),
            };
            IQueryable<ProductModel> queryable = Enumerable.Empty<Product>().AsQueryable().Select(p => new ProductModel
            {
                AlternateAddresses = p.AlternateAddresses.Select(i0 => new AlternateAddressModel
                {
                    Product = new ProductModel
                    {
                        AlternateAddresses = i0.Product.AlternateAddresses.Select(i1 => new AlternateAddressModel
                        {
                            Product = new ProductModel()
                        }).ToList()
                    }
                }).ToList()
            }).AsQueryable();
            var mapper = new Mapper(new MapperConfiguration(cfg => { }, NullLoggerFactory.Instance));

            // Act & Assert
            var exception =Assert.Throws<InvalidOperationException>(() => FilterUpdater.UpdaterExpansion(queryable.Expression, expansions, mapper));
            Assert.Equal("The last expansion in the list must have a filter.", exception.Message);
        }
    }
}

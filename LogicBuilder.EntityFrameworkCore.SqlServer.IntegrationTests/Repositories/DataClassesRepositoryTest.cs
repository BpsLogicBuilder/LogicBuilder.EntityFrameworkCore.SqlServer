using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.AutoMapperProfiles;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data.Stores;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models.Repositories;
using LogicBuilder.Expressions.Utils.Expansions;
using LogicBuilder.Expressions.Utils.ExpressionBuilder;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Lambda;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Logical;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Operand;
using LogicBuilder.Expressions.Utils.Strutures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Repositories
{
    public class DataClassesRepositoryTest
    {
        static DataClassesRepositoryTest()
        {
            InitializeMapperConfiguration();
        }

        public DataClassesRepositoryTest()
        {
            Initialize();
        }

        #region Fields
        private IServiceProvider serviceProvider;
        #endregion Fields

        [Fact]
        public async Task GetCategoryAsync_WithoutFilter_ReturnsAllEntities()
        {
            //arrange
            IDataClassesRepository repository = serviceProvider.GetRequiredService<IDataClassesRepository>();

            //act
            var categories = await repository.GetAsync<CategoryModel, Category>();

            //assert
            Assert.Equal(2, categories.Count);
        }

        [Fact]
        public async Task GetAsync_WithoutFilter_AndSingleObjectExpansion_ReturnsAllEntities()
        {
            //arrange
            IDataClassesRepository repository = serviceProvider.GetRequiredService<IDataClassesRepository>();
            var expansionDefinition = new SelectExpandDefinition
            (
                [],
                [
                    new SelectExpandItem
                    (
                        "Product",
                        null,
                        null,
                        [],
                        []
                    )
                ]
            );

            //act
            var addresses = await repository.GetAsync<AlternateAddressModel, AlternateAddress>(null, null, expansionDefinition);

            //assert
            Assert.True(addresses.Count > 0);
            Assert.True(addresses.All(a => a.Product != null));
        }

        [Fact]
        public async Task GetAsync_WithoutFilter_WithExpansions_ReturnsAllEntities_AndChildCollections()
        {
            //arrange
            IDataClassesRepository repository = serviceProvider.GetRequiredService<IDataClassesRepository>();
            var expansionDefinition = new SelectExpandDefinition
            (
                [],
                [
                    new SelectExpandItem
                    (
                        "Products",
                        null,
                        null,
                        [],
                        [
                            new SelectExpandItem
                            (
                                "AlternateAddresses",
                                null,
                                null,
                                [],
                                null
                            )
                        ]
                    )
                ]
            );

            //act
            var categories = await repository.GetAsync<CategoryModel, Category>(null, null, expansionDefinition);

            //assert
            Assert.Equal(2, categories.Count);
            Assert.Equal(3, categories.First().Products.Count);
            Assert.Equal(2, categories.Last().Products.Count);
            var product1 = categories.First().Products.First(p => p.ProductName == "ProductOne");
            var product4 = categories.Last().Products.First(p => p.ProductName == "ProductFour");
            Assert.Equal(2, product1.AlternateAddresses.Count);
            Assert.Equal(3, product4.AlternateAddresses.Count);
        }

        [Fact]
        public async Task GetAsync_WithFilterAndExpansions_ShouldFilterOnMembersNotInChildCollectionsSelect()
        {
            //arrange
            IDataClassesRepository repository = serviceProvider.GetRequiredService<IDataClassesRepository>();
            var parameters = new Dictionary<string, ParameterExpression>();
            var expansionDefinition = new SelectExpandDefinition
            (
                [],
                [
                    new SelectExpandItem
                    (
                        "Products",
                        new SelectExpandItemFilter
                        (
                            new FilterLambdaOperator
                            (
                                parameters,
                                new NotEqualsBinaryOperator
                                (
                                    new MemberSelectorOperator("ProductName", new ParameterOperator(parameters, "a")),
                                    new ConstantOperator("")
                                ),
                                typeof(ProductModel),
                                "a"
                            )
                        ),
                        null,
                        ["ProductID"],
                        [
                            new SelectExpandItem
                            (
                                "AlternateAddresses",
                                new SelectExpandItemFilter
                                (
                                    new FilterLambdaOperator
                                    (
                                        parameters,
                                        new NotEqualsBinaryOperator
                                        (
                                            new MemberSelectorOperator("City", new ParameterOperator(parameters, "a")),
                                            new ConstantOperator("")
                                        ),
                                        typeof(AlternateAddressModel),
                                        "a"
                                    )
                                ),
                                null,
                                ["State"],
                                null
                            )
                        ]
                    )
                ]
            );

            //act
            var categories = await repository.GetAsync<CategoryModel, Category>(c => c.CategoryName != "", null, expansionDefinition);

            //assert
            Assert.Equal(2, categories.Count);
            Assert.Equal(3, categories.First().Products.Count);
            Assert.Equal(2, categories.Last().Products.Count);
            var product1 = categories.First().Products.First(p => p.ProductID == 1);
            var product4 = categories.Last().Products.First(p => p.ProductID == 4);
            Assert.Equal(2, product1.AlternateAddresses.Count);
            Assert.Equal(3, product4.AlternateAddresses.Count);
        }

        [Fact]
        public async Task GetAsync_WithoutFilter_WithSortedExpansions_ReturnsAllEntities_AndChildCollections_InTheExpectedOrder()
        {
            //arrange
            IDataClassesRepository repository = serviceProvider.GetRequiredService<IDataClassesRepository>();
            var expansionDefinition = new SelectExpandDefinition
            (
                [],
                [
                    new SelectExpandItem
                    (
                        "Products",
                        null,
                        new SelectExpandItemQueryFunction
                        (
                            new SortCollection
                            (
                                [
                                    new SortDescription("ProductName", ListSortDirection.Descending)
                                ]
                            )
                        ),
                        [],
                        [
                            new SelectExpandItem
                            (
                                "AlternateAddresses",
                                null,
                                new SelectExpandItemQueryFunction
                                (
                                    new SortCollection
                                    (
                                        [
                                            new SortDescription("City", ListSortDirection.Descending)
                                        ]
                                    )
                                ),
                                [],
                                null
                            )
                        ]
                    )
                ]
            );

            //act
            var categories = await repository.GetAsync<CategoryModel, Category>(null, null, expansionDefinition);

            //assert
            Assert.Equal(2, categories.Count);
            Assert.Equal(3, categories.First().Products.Count);
            Assert.Equal(2, categories.Last().Products.Count);
            Assert.Equal("ProductTwo", categories.First().Products.First().ProductName);
            Assert.Equal("ProductOne", categories.First().Products.Last().ProductName);
            Assert.Equal("ProductFour", categories.Last().Products.First().ProductName);
            Assert.Equal("ProductFive", categories.Last().Products.Last().ProductName);
            var product1 = categories.First().Products.First(p => p.ProductName == "ProductOne");
            var product4 = categories.Last().Products.First(p => p.ProductName == "ProductFour");
            Assert.Equal(2, product1.AlternateAddresses.Count);
            Assert.Equal(3, product4.AlternateAddresses.Count);
            Assert.Equal("CityTwo", product1.AlternateAddresses.First().City);
            Assert.Equal("CityOne", product1.AlternateAddresses.Last().City);
            Assert.Equal("CityThree", product4.AlternateAddresses.First().City);
            Assert.Equal("CityFive", product4.AlternateAddresses.Last().City);
        }

        [Fact]
        public async Task GetAsync_WithoutFilters_OnChildCollections_ReturnsTheExpectedEntries()
        {
            //arrange
            IDataClassesRepository repository = serviceProvider.GetRequiredService<IDataClassesRepository>();
            var parameters = new Dictionary<string, ParameterExpression>();
            var expansionDefinition = new SelectExpandDefinition
            (
                [],
                [
                    new SelectExpandItem
                    (
                        "Products",
                        new SelectExpandItemFilter
                        (
                            new FilterLambdaOperator
                            (
                                parameters,
                                new OrBinaryOperator
                                (
                                    new EqualsBinaryOperator
                                    (
                                        new MemberSelectorOperator("ProductName", new ParameterOperator(parameters, "a")),
                                        new ConstantOperator("ProductOne")
                                    ),
                                    new EqualsBinaryOperator
                                    (
                                        new MemberSelectorOperator("ProductName", new ParameterOperator(parameters, "a")),
                                        new ConstantOperator("ProductFour")
                                    )
                                ),
                                typeof(ProductModel),
                                "a"
                            )
                        ),
                        null,
                        [],
                        [
                            new SelectExpandItem
                            (
                                "AlternateAddresses",
                                new SelectExpandItemFilter
                                (
                                    new FilterLambdaOperator
                                    (
                                        parameters,
                                        new OrBinaryOperator
                                        (
                                            new EqualsBinaryOperator
                                            (
                                                new MemberSelectorOperator("City", new ParameterOperator(parameters, "b")),
                                                new ConstantOperator("CityOne")
                                            ),
                                            new EqualsBinaryOperator
                                            (
                                                new MemberSelectorOperator("City", new ParameterOperator(parameters, "b")),
                                                new ConstantOperator("CityFive")
                                            )
                                        ),
                                        typeof(AlternateAddressModel),
                                        "b"
                                    )
                                ),
                                null,
                                [],
                                null
                            )
                        ]
                    )
                ]
            );

            //act
            var categories = await repository.GetAsync<CategoryModel, Category>(null, null, expansionDefinition);

            //assert
            Assert.Equal(2, categories.Count);
            Assert.Single(categories.First().Products);
            Assert.Single(categories.Last().Products);
            var product1 = categories.First().Products.Single();
            var product4 = categories.Last().Products.Single();
            Assert.Equal("ProductOne", product1.ProductName);
            Assert.Equal("ProductFour", product4.ProductName);
            Assert.Single(product1.AlternateAddresses);
            Assert.Single(product4.AlternateAddresses);
            var city1 = product1.AlternateAddresses.Single();
            var city5 = product4.AlternateAddresses.Single();
            Assert.Equal("CityOne", city1.City);
            Assert.Equal("CityFive", city5.City);
        }

        [Fact]
        public async Task GetAsync_ShouldSortMultipleExpansionsOfTheSameCollectionType()
        {
            //arrange
            IDataClassesRepository repository = serviceProvider.GetRequiredService<IDataClassesRepository>();
            var expansionDefinition = new SelectExpandDefinition
            (
                ["ProductID", "Category", "AlternateAddresses"],
                [
                    new SelectExpandItem
                    (
                        "Category",
                        null,
                        null,
                        ["CategoryName", "Products"],
                        [
                            new SelectExpandItem
                            (
                                "Products",
                                null,
                                null,
                                ["ProductID", "AlternateAddresses"],
                                [
                                    new SelectExpandItem
                                    (
                                        "AlternateAddresses",
                                        null,
                                        new SelectExpandItemQueryFunction
                                        (
                                            new SortCollection
                                            (
                                                [
                                                    new SortDescription("City", ListSortDirection.Descending)
                                                ]
                                            )
                                        ),
                                        ["Product", "City"],
                                        [
                                            new SelectExpandItem
                                            (
                                                "Product",
                                                null,
                                                null,
                                                [],
                                                []
                                            )
                                        ]
                                    )
                                ]
                            )
                        ]
                    ),
                    new SelectExpandItem
                    (
                        "AlternateAddresses",
                        null,
                        new SelectExpandItemQueryFunction
                        (
                            new SortCollection
                            (
                                [
                                    new SortDescription("City", ListSortDirection.Descending)
                                ]
                            )
                        ),
                        ["Product", "City"],
                        [
                            new SelectExpandItem
                            (
                                "Product",
                                null,
                                null,
                                ["ProductID", "AlternateAddresses"],
                                [
                                    new SelectExpandItem
                                    (
                                        "AlternateAddresses",
                                        null,
                                        new SelectExpandItemQueryFunction
                                        (
                                            new SortCollection
                                            (
                                                [
                                                    new SortDescription("City", ListSortDirection.Descending)
                                                ]
                                            )
                                        ),
                                        ["Product", "City"],
                                        [
                                            new SelectExpandItem
                                            (
                                                "Product",
                                                null,
                                                null,
                                                [],
                                                []
                                            )
                                        ]
                                    )
                                ]
                            )
                        ]
                    )
                ]
            );

            //act
            var products = (await repository.GetAsync<ProductModel, Product>(null, null, expansionDefinition)).ToArray();

            //assert
            Assert.True(products.Length > 0);
            Assert.Equal("CityTwo", products[0].AlternateAddresses.First().City);
            Assert.Equal("CityOne", products[0].AlternateAddresses.Last().City);

            Assert.Equal("CityTwo", products[0].Category.Products.First().AlternateAddresses.First().City);
            Assert.Equal("CityOne", products[0].Category.Products.First().AlternateAddresses.Last().City);
        }

        #region Helpers

        private static void InitializeMapperConfiguration()
        {
            MapperConfiguration ??= ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();
                cfg.AddProfile<DataClassesMappings>();
            });
        }

        static MapperConfiguration MapperConfiguration;
        private void Initialize()
        {
            MapperConfiguration.AssertConfigurationIsValid();
            serviceProvider = new ServiceCollection()
                .AddDbContext<DataClassesContext>
                (
                    options => options.UseSqlServer
                    (
                        @"Server=(localdb)\mssqllocaldb;Database=DataClassesRepositoryTest;ConnectRetryCount=0",
                        options => options.EnableRetryOnFailure()
                    ),
                    ServiceLifetime.Transient
                )
                .AddTransient<IDataClassesStore, DataClassesStore>()
                .AddTransient<IDataClassesRepository, DataClassesRepository>()
                .AddSingleton<AutoMapper.IConfigurationProvider>
                (
                    MapperConfiguration
                )
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<AutoMapper.IConfigurationProvider>(), sp.GetService))
                .BuildServiceProvider();

            ReCreateDataBase(serviceProvider.GetRequiredService<DataClassesContext>());
            DatabaseSeeder.Seed_Database(serviceProvider.GetRequiredService<IDataClassesRepository>()).Wait();
        }

        private static void ReCreateDataBase(DataClassesContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
        #endregion Helpers
    }
}

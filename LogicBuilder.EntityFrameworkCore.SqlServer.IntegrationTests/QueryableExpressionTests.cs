using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.AutoMapperProfiles;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data.Stores;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models.Repositories;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Lambda;
using LogicBuilder.Expressions.Utils.ExpressionDescriptors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests
{
    public class QueryableExpressionTests
    {
        static QueryableExpressionTests()
        {
            InitializeMapperConfiguration();
        }

        public QueryableExpressionTests()
        {
            Initialize();
        }

        [Fact]
        public async Task Select_Group_Students_By_EnrollmentDate_Return_EnrollmentDate_With_Count()
        {
            //arrange
            var selectorLambdaOperatorDescriptor = GetExpressionDescriptor<IQueryable<StudentModel>, IQueryable<LookUpsModel>>
            (
                GetAboutBody(),
                "q"
            );
            var expression = GetExpression<IQueryable<StudentModel>, IQueryable<LookUpsModel>>(selectorLambdaOperatorDescriptor);

            //act
            IQueryable<LookUpsModel> queryableResult = await serviceProvider.GetRequiredService<ISchoolRepository>().QueryAsync<StudentModel, Student, IQueryable<LookUpsModel>, IQueryable<LookUps>>(expression);
            var result = await queryableResult.ToListAsync(CancellationToken.None);

            //assert
            AssertFilterStringIsCorrect(expression, "q => q.GroupBy(item => item.EnrollmentDate).OrderByDescending(group => group.Key).Select(sel => new LookUpsModel() {DateTimeValue = sel.Key, NumericValue = Convert(sel.AsQueryable().Count())})");
            Assert.Equal(6, result.Count);
        }

        #region Fields
        private IServiceProvider serviceProvider;
        #endregion Fields

        #region Helpers
        private static SelectDescriptor GetAboutBody()
            => new
            (
                new OrderByDescriptor
                (
                    new GroupByDescriptor
                    (
                        new ParameterDescriptor("q"),
                        new MemberSelectorDescriptor
                        (
                            "EnrollmentDate",
                            new ParameterDescriptor("item")
                        ),
                        "item"
                    ),
                    new MemberSelectorDescriptor
                    (
                        "Key",
                        new ParameterDescriptor("group")
                    ),
                    LogicBuilder.Expressions.Utils.Strutures.ListSortDirection.Descending,
                    "group"
                ),
                new MemberInitDescriptor
                (
                    new Dictionary<string, DescriptorBase>
                    {
                        ["DateTimeValue"] = new MemberSelectorDescriptor
                        (
                            "Key",
                            new ParameterDescriptor("sel")
                        ),
                        ["NumericValue"] = new ConvertDescriptor
                        (
                            new CountDescriptor
                            (
                                new AsQueryableDescriptor(new ParameterDescriptor("sel"))

                            ),
                            typeof(double?).AssemblyQualifiedName
                        )
                    },
                    typeof(LookUpsModel).AssemblyQualifiedName
                ),
                "sel"
            );

        private static SelectorLambdaDescriptor GetExpressionDescriptor<T, TResult>(DescriptorBase selectorBody, string parameterName = "$it")
            => new
            (
                selectorBody,
                typeof(T).AssemblyQualifiedName,
                parameterName,
                typeof(TResult).AssemblyQualifiedName
            );

        private Expression<Func<T, TResult>> GetExpression<T, TResult>(SelectorLambdaDescriptor selectorLambdaDescriptor)
        {
            IMapper mapper = serviceProvider.GetRequiredService<IMapper>();

            return (Expression<Func<T, TResult>>)mapper.Map<SelectorLambdaOperator>
            (
                selectorLambdaDescriptor,
                opts => opts.Items["parameters"] = new Dictionary<string, ParameterExpression>()
            ).Build();
        }

        private static void AssertFilterStringIsCorrect(Expression expression, string expected)
        {
            string resultExpression = ExpressionStringBuilder.ToString(expression);
            Assert.True(expected == resultExpression, string.Format("Expected expression '{0}' but the deserializer produced '{1}'", expected, resultExpression));
        }

        private static void InitializeMapperConfiguration()
        {
            MapperConfiguration ??= ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.AddProfile<SchoolProfile>();
                cfg.AddProfile<Mapping.ExpressionOperatorsMappingProfile>();
            });
        }

        static MapperConfiguration MapperConfiguration;
        private void Initialize()
        {
            MapperConfiguration.AssertConfigurationIsValid();
            serviceProvider = new ServiceCollection()
                .AddDbContext<SchoolContext>
                (
                    options => options.UseSqlServer
                    (
                        @"Server=(localdb)\mssqllocaldb;Database=Integration_QueryableExpressionTests;ConnectRetryCount=0",
                        options => options.EnableRetryOnFailure()
                    ),
                    ServiceLifetime.Transient
                )
                .AddTransient<ISchoolStore, SchoolStore>()
                .AddTransient<ISchoolRepository, SchoolRepository>()
                .AddSingleton<AutoMapper.IConfigurationProvider>
                (
                    MapperConfiguration
                )
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<AutoMapper.IConfigurationProvider>(), sp.GetService))
                .BuildServiceProvider();

            ReCreateDataBase(serviceProvider.GetRequiredService<SchoolContext>());
            DatabaseSeeder.Seed_Database(serviceProvider.GetRequiredService<ISchoolRepository>()).Wait();
        }

        private static void ReCreateDataBase(SchoolContext context)
        {
            context.Database.EnsureCreated();
        }
        #endregion Helpers
    }
}
using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.AutoMapperProfiles;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data.Stores;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models.Repositories;
using LogicBuilder.Expressions.Utils.ExpressionBuilder.Lambda;
using LogicBuilder.Expressions.Utils.ExpressionDescriptors;
using LogicBuilder.Expressions.Utils.Strutures;
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

        [Fact]
        public async Task BuildGroup_Departments_By_OrderBy_ThenBy_Skip_Take_Average()
        {
            //arrange
            var bodyParameter = new SelectDescriptor
            (
                new OrderByDescriptor
                (
                    new GroupByDescriptor
                    (
                        new ParameterDescriptor("q"),
                        new ConstantDescriptor(1, typeof(int).AssemblyQualifiedName),
                        "a"
                    ),
                    new MemberSelectorDescriptor("Key", new ParameterDescriptor("b")),
                    ListSortDirection.Ascending,
                    "b"
                ),
                new MemberInitDescriptor
                (
                    new Dictionary<string, DescriptorBase>
                    {
                        ["Sum_budget"] = new SumDescriptor
                        (
                            new WhereDescriptor
                            (
                                new ParameterDescriptor("q"),
                                new AndBinaryDescriptor
                                (
                                    new NotEqualsBinaryDescriptor
                                    (
                                        new MemberSelectorDescriptor("DepartmentID", new ParameterDescriptor("d")),
                                        new CountDescriptor(new ParameterDescriptor("q"))
                                    ),
                                    new EqualsBinaryDescriptor
                                    (
                                        new MemberSelectorDescriptor("DepartmentID", new ParameterDescriptor("d")),
                                        new MemberSelectorDescriptor("Key", new ParameterDescriptor("c"))
                                    )
                                ),
                                "d"
                            ),
                            new MemberSelectorDescriptor("Budget", new ParameterDescriptor("item")),
                            "item"
                        )
                    }
                ),
                "c"
            );

            var selectorLambdaOperatorDescriptor = GetExpressionDescriptor<IQueryable<DepartmentModel>, IQueryable<dynamic>>
            (
                bodyParameter,
                "q"
            );
            var expression = GetExpression<IQueryable<DepartmentModel>, IQueryable<dynamic>>(selectorLambdaOperatorDescriptor);

            //act
            IQueryable<dynamic> queryableResult = await serviceProvider.GetRequiredService<ISchoolRepository>().QueryAsync<DepartmentModel, Department, IQueryable<dynamic>, IQueryable<dynamic>>(expression);
            var result = await queryableResult.ToListAsync(CancellationToken.None);

            //assert
            AssertFilterStringIsCorrect(expression, "q => Convert(q.GroupBy(a => 1).OrderBy(b => b.Key).Select(c => new AnonymousType() {Sum_budget = q.Where(d => ((d.DepartmentID != q.Count()) AndAlso (d.DepartmentID == c.Key))).Sum(item => item.Budget)}))");
            Assert.True(result.First().Sum_budget == 350000);
        }

        [Fact]
        public async Task Group_Courses_By_Select()
        {
            //arrange
            string parameterName = "$it";
            var bodyParameter = new SelectDescriptor
            (
                new GroupByDescriptor
                (
                    new ParameterDescriptor(parameterName),
                    new MemberSelectorDescriptor("DepartmentID", new ParameterDescriptor("a")),
                    "a"
                ),
                new MemberSelectorDescriptor("Key", new ParameterDescriptor("b")),
                "b"
            );

            var selectorLambdaOperatorDescriptor = GetExpressionDescriptor<IQueryable<CourseModel>, IQueryable<int>>
            (
                bodyParameter,
                parameterName
            );
            var expression = GetExpression<IQueryable<CourseModel>, IQueryable<int>>(selectorLambdaOperatorDescriptor);

            //act
            IQueryable<int> queryableResult = await serviceProvider.GetRequiredService<ISchoolRepository>().QueryAsync<CourseModel, Course, IQueryable<int>, IQueryable<int>>(expression);
            var result = await queryableResult.ToListAsync(CancellationToken.None);

            //assert
            AssertFilterStringIsCorrect(expression, "$it => $it.GroupBy(a => a.DepartmentID).Select(b => b.Key)");
            Assert.NotNull(result);
        }

        [Fact]
        public async Task Order_Departments_Select_New_Anonymoustype()
        {
            //arrange
            string parameterName = "$it";
            var bodyParameter = new SelectDescriptor
            (
                new OrderByDescriptor
                (
                    new ParameterDescriptor(parameterName),
                    new MemberSelectorDescriptor("DepartmentID", new ParameterDescriptor("a")),
                    ListSortDirection.Descending,
                    "a"
                ),
                new MemberInitDescriptor
                (
                    new Dictionary<string, DescriptorBase>
                    {
                        ["ID"] = new MemberSelectorDescriptor("DepartmentID", new ParameterDescriptor("a")),
                        ["DepartmentName"] = new MemberSelectorDescriptor("Name", new ParameterDescriptor("a")),
                        ["Courses"] = new MemberSelectorDescriptor("Courses", new ParameterDescriptor("a"))
                    }
                ),
                "a"
            );

            var selectorLambdaOperatorDescriptor = GetExpressionDescriptor<IQueryable<DepartmentModel>, IQueryable<dynamic>>
            (
                bodyParameter,
                parameterName
            );
            var expression = GetExpression<IQueryable<DepartmentModel>, IQueryable<dynamic>>(selectorLambdaOperatorDescriptor);

            //act
            IQueryable<dynamic> queryableResult = await serviceProvider.GetRequiredService<ISchoolRepository>().QueryAsync<DepartmentModel, Department, IQueryable<dynamic>, IQueryable<dynamic>>(expression);
            var result = await queryableResult.ToListAsync(CancellationToken.None);

            //assert
            AssertFilterStringIsCorrect(expression, "$it => Convert($it.OrderByDescending(a => a.DepartmentID).Select(a => new AnonymousType() {ID = a.DepartmentID, DepartmentName = a.Name, Courses = a.Courses}))");
            Assert.Equal(4, result.First().ID);
        }

        [Fact]
        public async Task SelectInstructorFullNames()
        {
            //arrange
            string parameterName = "q";
            var bodyParameter = new SelectDescriptor
            (
                new OrderByDescriptor
                (
                    new ParameterDescriptor(parameterName),
                    new MemberSelectorDescriptor
                    (
                        "FullName",
                        new ParameterDescriptor("s")
                    ),
                    ListSortDirection.Ascending,
                    "s"
                ),
                new MemberSelectorDescriptor("FullName", new ParameterDescriptor("a")),
                "a"
            );

            var selectorLambdaOperatorDescriptor = GetExpressionDescriptor<IQueryable<InstructorModel>, IQueryable<string>>
            (
                bodyParameter,
                parameterName
            );
            var expression = GetExpression<IQueryable<InstructorModel>, IQueryable<string>>(selectorLambdaOperatorDescriptor);

            //act
            IQueryable<string> queryableResult = await serviceProvider.GetRequiredService<ISchoolRepository>().QueryAsync<InstructorModel, Instructor, IQueryable<string>, IQueryable<string>>(expression);
            var result = await queryableResult.ToListAsync(CancellationToken.None);

            //assert
            AssertFilterStringIsCorrect(expression, "q => q.OrderBy(s => s.FullName).Select(a => a.FullName)");
            Assert.Equal("Candace Kapoor", result.First());
        }

        [Fact]
        public async Task Order_Instructors_Select_New_Anonymoustype_FullNameOnly()
        {
            //arrange
            string parameterName = "q";
            var bodyParameter = new SelectDescriptor
            (
                new OrderByDescriptor
                (
                    new ParameterDescriptor(parameterName),
                    new MemberSelectorDescriptor
                    (
                        "FullName",
                        new ParameterDescriptor("s")
                    ),
                    ListSortDirection.Ascending,
                    "s"
                ),
                new MemberInitDescriptor
                (
                    new Dictionary<string, DescriptorBase>
                    {
                        ["FullName"] = new MemberSelectorDescriptor("FullName", new ParameterDescriptor("a"))
                    }
                ),
                "a"
            );

            var selectorLambdaOperatorDescriptor = GetExpressionDescriptor<IQueryable<InstructorModel>, IQueryable<dynamic>>
            (
                bodyParameter,
                parameterName
            );
            var expression = GetExpression<IQueryable<InstructorModel>, IQueryable<dynamic>>(selectorLambdaOperatorDescriptor);

            //act
            IQueryable<dynamic> queryableResult = await serviceProvider.GetRequiredService<ISchoolRepository>().QueryAsync<InstructorModel, Instructor, IQueryable<dynamic>, IQueryable<dynamic>>(expression);
            var result = await queryableResult.ToListAsync(CancellationToken.None);

            //assert
            AssertFilterStringIsCorrect(expression, "q => Convert(q.OrderBy(s => s.FullName).Select(a => new AnonymousType() {FullName = a.FullName}))");
            Assert.Equal("Candace Kapoor", result.First().FullName);
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
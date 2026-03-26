using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.AutoMapperProfiles;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data.Stores;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests
{
    public class PersistenceTest
    {
        static PersistenceTest()
        {
            InitializeMapperConfiguration();
        }

        public PersistenceTest()
        {
            Initialize();
        }

        #region Fields
        private IServiceProvider serviceProvider;
        #endregion Fields

        [Fact]
        public async Task SaveDepartmentSpecifyingRowVersionExpansion()
        {
            //arrange
            ISchoolRepository schoolRepository = serviceProvider.GetRequiredService<ISchoolRepository>();
            var department = (await schoolRepository.GetAsync<DepartmentModel, Department>
            (
                s => s.Name == "Mathematics",
                selectExpandDefinition: new Expressions.Utils.Expansions.SelectExpandDefinition
                (
                    [],
                    [//Need expansion because RowVersion is not a literal type (included without explicit expansion)
                        new("RowVersion")
                    ]
                )
            )).Single();

            department.Budget = 1000.1m;
            department.EntityState = Domain.EntityStateType.Modified;

            //act
            bool result = await schoolRepository.SaveAsync<DepartmentModel, Department>(department);
            var savedDepartment = (await schoolRepository.GetAsync<DepartmentModel, Department>
            (
                s => s.Name == "Mathematics"
            )).Single();

            //assert
            Assert.True(result);
            Assert.Equal(1000.1m, savedDepartment.Budget);
        }

        [Fact]
        public async Task SaveDepartmentWithoutSpecifyingRowVersionExpansion()
        {
            //arrange
            ISchoolRepository schoolRepository = serviceProvider.GetRequiredService<ISchoolRepository>();
            var department = (await schoolRepository.GetAsync<DepartmentModel, Department>
            (
                s => s.Name == "Mathematics"
            )).Single();
            department.Budget = 1000.1m;
            department.EntityState = LogicBuilder.Domain.EntityStateType.Modified;

            bool result = await schoolRepository.SaveAsync<DepartmentModel, Department>(department);
            var savedDepartment = (await schoolRepository.GetAsync<DepartmentModel, Department>
            (
                s => s.Name == "Mathematics"
            )).Single();

            //assert
            Assert.True(result);
            Assert.Equal(1000.1m, savedDepartment.Budget);
        }

        [Fact]
        public async Task CanUpdateAnObjectThenDeleteIt()
        {
            ISchoolRepository schoolRepository = serviceProvider.GetRequiredService<ISchoolRepository>();
            var student = (await schoolRepository.GetAsync<StudentModel, Student>
            (
                s => s.FullName == "Carson Alexander"
            )).Single();
            int id = student.ID;
            student.FirstName = "First";
            student.EntityState = LogicBuilder.Domain.EntityStateType.Modified;

            bool success = await schoolRepository.SaveGraphAsync<StudentModel, Student>(student);
            var student2 = (await schoolRepository.GetAsync<StudentModel, Student>
            (
                f => f.ID == id
            )).SingleOrDefault();

            Assert.Equal("First", student2.FirstName);
            Assert.True(success);
            student2.EntityState = LogicBuilder.Domain.EntityStateType.Deleted;

            success = await schoolRepository.SaveGraphAsync<StudentModel, Student>(student2);
            var student3 = (await schoolRepository.GetAsync<StudentModel, Student>
            (
                f => f.ID == id
            )).SingleOrDefault();
            Assert.True(success);
            Assert.Null(student3);
        }

        [Fact]
        public async Task CanUpdateAChildObjectThenDeleteIt()
        {
            ISchoolRepository schoolRepository = serviceProvider.GetRequiredService<ISchoolRepository>();
            var instructor = (await schoolRepository.GetAsync<InstructorModel, Instructor>
            (
                s => s.FullName == "Candace Kapoor",
                null,
                new LogicBuilder.Expressions.Utils.Expansions.SelectExpandDefinition
                (
                    [],
                    [
                        new LogicBuilder.Expressions.Utils.Expansions.SelectExpandItem(nameof(InstructorModel.OfficeAssignment))
                    ]
                )
            )).First();

            int id = instructor.ID;
            instructor.OfficeAssignment.Location = "Location1";
            instructor.EntityState = Domain.EntityStateType.Modified;
            instructor.OfficeAssignment.EntityState = Domain.EntityStateType.Modified;

            bool success = await schoolRepository.SaveGraphAsync<InstructorModel, Instructor>(instructor);
            var instructor2 = (await schoolRepository.GetAsync<InstructorModel, Instructor>
            (
                f => f.ID == id,
                null,
                new Expressions.Utils.Expansions.SelectExpandDefinition
                (
                    [],
                    [
                        new Expressions.Utils.Expansions.SelectExpandItem(nameof(InstructorModel.OfficeAssignment))
                    ]
                )
            )).First();

            Assert.Equal("Location1", instructor2.OfficeAssignment.Location);
            Assert.True(success);
            instructor.EntityState = Domain.EntityStateType.Modified;
            instructor.OfficeAssignment.EntityState = Domain.EntityStateType.Deleted;

            success = await schoolRepository.SaveGraphAsync<InstructorModel, Instructor>(instructor);
            var instructor3 = (await schoolRepository.GetAsync<InstructorModel, Instructor>
            (
                f => f.ID == id,
                null,
                new Expressions.Utils.Expansions.SelectExpandDefinition
                (
                    [],
                    [
                        new Expressions.Utils.Expansions.SelectExpandItem(nameof(InstructorModel.OfficeAssignment))
                    ]
                )
            )).First();

            Assert.Null(instructor3.OfficeAssignment);
            Assert.True(success);
        }

        #region Helpers

        private static void InitializeMapperConfiguration()
        {
            MapperConfiguration ??= ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.AddProfile<SchoolProfile>();
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
                        @"Server=(localdb)\mssqllocaldb;Database=PersistenceTest;ConnectRetryCount=0",
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
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
        #endregion Helpers
    }
}

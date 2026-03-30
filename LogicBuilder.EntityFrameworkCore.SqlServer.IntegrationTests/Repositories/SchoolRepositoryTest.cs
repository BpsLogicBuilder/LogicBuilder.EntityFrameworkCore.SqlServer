using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.AutoMapperProfiles;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data.Stores;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models.Repositories;
using LogicBuilder.Expressions.Utils.Expansions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Repositories
{
    public class SchoolRepositoryTest
    {
        static SchoolRepositoryTest()
        {
            InitializeMapperConfiguration();
        }

        public SchoolRepositoryTest()
        {
            Initialize();
        }

        #region Fields
        private IServiceProvider serviceProvider;
        #endregion Fields

        [Fact]
        public async Task GetStudentMsAsync_WithFilter_ReturnsMatchingEntities()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();

            //act
            var students = await repository.GetAsync<StudentModel, Student>(s => s.LastName == "Alexander");

            //assert
            Assert.NotNull(students);
            Assert.Single(students);
            Assert.Equal("Carson", students.First().FirstName);
        }

        [Fact]
        public async Task GetStudentsAsync_WithAQueryExpression_ReturnsAlItemsInTheExpectedOrder()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();

            //act
            var students = await repository.GetAsync<StudentModel, Student>(null, q => q.OrderByDescending(s => s.LastName).ThenBy(s => s.FirstName));

            //assert
            Assert.NotEmpty(students);
            Assert.Equal("Billie", students.First().FirstName);
            Assert.All(students, s => Assert.Null(s.Enrollments));
        }

        [Fact]
        public async Task GetStudentsAsync_WithExpansion_ReturnsChildCollections()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();

            //act
            var students = await repository.GetAsync<StudentModel, Student>
            (
                null,
                null,
                new SelectExpandDefinition
                (
                    [],
                    [
                        new SelectExpandItem(nameof(StudentModel.Enrollments))
                    ]
                )
            );

            //assert
            Assert.NotEmpty(students);
            Assert.NotEmpty(students.First().Enrollments);
        }


        [Fact]
        public async Task QueryAsync_WorksWenModelReturnTypeAndDataReturnTypeAreTheSame()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();

            //act
            var count = await repository.QueryAsync<StudentModel, Student, int, int>(q => q.Count(s => s.LastName != "Spratt"));

            //assert
            Assert.True(count > 5);
        }

        [Fact]
        public async Task QueryAsync_WorksWenModelReturnTypeAndDataReturnTypeAreNotQueryables()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();

            //act
            var students = await repository.QueryAsync<StudentModel, Student, List<StudentModel>, List<Student>>(q => q.ToList());

            //assert
            Assert.NotEmpty(students);
            Assert.True(students.Count > 5);
        }

        [Fact]
        public async Task GetAsync_WithoutFilter_ReturnsAllEntities()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();

            //act
            var students = await repository.GetAsync<StudentModel, Student>();

            //assert
            Assert.NotNull(students);
            Assert.True(students.Count > 0);
        }

        [Fact]
        public async Task GetAsync_WithSelectExpandDefinition_IncludesNavigationProperties()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();

            //act
            var instructors = await repository.GetAsync<InstructorModel, Instructor>
            (
                s => s.FullName == "Candace Kapoor",
                null,
                new SelectExpandDefinition
                (
                    [],
                    [
                        new SelectExpandItem(nameof(InstructorModel.OfficeAssignment))
                    ]
                )
            );

            //assert
            Assert.Single(instructors);
            Assert.NotNull(instructors.First().OfficeAssignment);
        }

        [Fact]
        public async Task CountAsync_WithFilter_ReturnsCorrectCount()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();

            //act
            int count = await repository.CountAsync<StudentModel, Student>(s => s.LastName == "Alexander");

            //assert
            Assert.Equal(1, count);
        }

        [Fact]
        public async Task CountAsync_WithoutFilter_ReturnsAllCount()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();

            //act
            int count = await repository.CountAsync<StudentModel, Student>();

            //assert
            Assert.True(count > 0);
        }

        [Fact]
        public async Task SaveAsync_SingleEntity_AddsNewEntity()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();
            var newStudent = new StudentModel
            {
                FirstName = "John",
                LastName = "Doe",
                EnrollmentDate = DateTime.UtcNow,
                EntityState = Domain.EntityStateType.Added
            };

            //act
            bool result = await repository.SaveAsync<StudentModel, Student>(newStudent);
            var savedStudent = (await repository.GetAsync<StudentModel, Student>(s => s.FirstName == "John" && s.LastName == "Doe")).SingleOrDefault();

            //assert
            Assert.True(result);
            Assert.NotNull(savedStudent);
            Assert.Equal("John", savedStudent.FirstName);
        }

        [Fact]
        public async Task SaveAsync_SingleEntity_UpdatesExistingEntity()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();
            var student = (await repository.GetAsync<StudentModel, Student>(s => s.FullName == "Carson Alexander")).Single();
            int id = student.ID;
            student.FirstName = "UpdatedName";
            student.EntityState = Domain.EntityStateType.Modified;

            //act
            bool result = await repository.SaveAsync<StudentModel, Student>(student);
            var updatedStudent = (await repository.GetAsync<StudentModel, Student>(s => s.ID == id)).Single();

            //assert
            Assert.True(result);
            Assert.Equal("UpdatedName", updatedStudent.FirstName);
        }

        [Fact]
        public async Task SaveAsync_Collection_SavesMultipleEntities()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();
            var students = new[]
            {
                new StudentModel
                {
                    FirstName = "Jane",
                    LastName = "Smith",
                    EnrollmentDate = DateTime.UtcNow,
                    EntityState = Domain.EntityStateType.Added
                },
                new StudentModel
                {
                    FirstName = "Bob",
                    LastName = "Johnson",
                    EnrollmentDate = DateTime.UtcNow,
                    EntityState = Domain.EntityStateType.Added
                }
            };

            //act
            bool result = await repository.SaveAsync<StudentModel, Student>(students);
            var savedStudents = await repository.GetAsync<StudentModel, Student>(s => s.LastName == "Smith" || s.LastName == "Johnson");

            //assert
            Assert.True(result);
            Assert.Equal(2, savedStudents.Count);
        }

        [Fact]
        public async Task SaveGraphAsync_SingleEntity_SavesEntityWithNavigationProperties()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();
            var instructor = (await repository.GetAsync<InstructorModel, Instructor>
            (
                s => s.FullName == "Candace Kapoor",
                null,
                new SelectExpandDefinition
                (
                    [],
                    [
                        new SelectExpandItem(nameof(InstructorModel.OfficeAssignment))
                    ]
                )
            )).First();
            int id = instructor.ID;
            instructor.OfficeAssignment.Location = "UpdatedLocation";
            instructor.EntityState = Domain.EntityStateType.Modified;
            instructor.OfficeAssignment.EntityState = Domain.EntityStateType.Modified;

            //act
            bool result = await repository.SaveGraphAsync<InstructorModel, Instructor>(instructor);
            var savedInstructor = (await repository.GetAsync<InstructorModel, Instructor>
            (
                f => f.ID == id,
                null,
                new SelectExpandDefinition
                (
                    [],
                    [
                        new SelectExpandItem(nameof(InstructorModel.OfficeAssignment))
                    ]
                )
            )).First();

            //assert
            Assert.True(result);
            Assert.Equal("UpdatedLocation", savedInstructor.OfficeAssignment.Location);
        }

        [Fact]
        public async Task SaveGraphsAsync_Collection_SavesMultipleGraphs()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();
            var instructors = await repository.GetAsync<InstructorModel, Instructor>
            (
                null,
                null,
                new SelectExpandDefinition
                (
                    [],
                    [
                        new SelectExpandItem(nameof(InstructorModel.OfficeAssignment))
                    ]
                )
            );

            foreach (var instructor in instructors.Where(i => i.OfficeAssignment != null))
            {
                instructor.OfficeAssignment.Location += "_Updated";
                instructor.EntityState = Domain.EntityStateType.Modified;
                instructor.OfficeAssignment.EntityState = Domain.EntityStateType.Modified;
            }

            //act
            bool result = await repository.SaveGraphsAsync<InstructorModel, Instructor>([.. instructors.Where(i => i.OfficeAssignment != null)]);

            //assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_RemovesEntitiesMatchingFilter()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();
            var newStudent = new StudentModel
            {
                FirstName = "ToDelete",
                LastName = "Student",
                EnrollmentDate = DateTime.UtcNow,
                EntityState = Domain.EntityStateType.Added
            };
            await repository.SaveAsync<StudentModel, Student>(newStudent);

            //act
            bool result = await repository.DeleteAsync<StudentModel, Student>(s => s.FirstName == "ToDelete");
            var deletedStudent = (await repository.GetAsync<StudentModel, Student>(s => s.FirstName == "ToDelete")).SingleOrDefault();

            //assert
            Assert.True(result);
            Assert.Null(deletedStudent);
        }

        [Fact]
        public async Task AddChange_AndSaveChangesAsync_PersistsChange()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();
            var newStudent = new StudentModel
            {
                FirstName = "AddChange",
                LastName = "Test",
                EnrollmentDate = DateTime.UtcNow,
                EntityState = Domain.EntityStateType.Added
            };

            //act
            repository.AddChange<StudentModel, Student>(newStudent);
            bool result = await repository.SaveChangesAsync();
            var savedStudent = (await repository.GetAsync<StudentModel, Student>(s => s.FirstName == "AddChange")).SingleOrDefault();

            //assert
            Assert.True(result);
            Assert.NotNull(savedStudent);
        }

        [Fact]
        public async Task AddChanges_Collection_AndSaveChangesAsync_PersistsAllChanges()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();
            var students = new[]
            {
                new StudentModel
                {
                    FirstName = "Multi1",
                    LastName = "Change",
                    EnrollmentDate = DateTime.UtcNow,
                    EntityState = Domain.EntityStateType.Added
                },
                new StudentModel
                {
                    FirstName = "Multi2",
                    LastName = "Change",
                    EnrollmentDate = DateTime.UtcNow,
                    EntityState = Domain.EntityStateType.Added
                }
            };

            //act
            repository.AddChanges<StudentModel, Student>(students);
            bool result = await repository.SaveChangesAsync();
            var savedStudents = await repository.GetAsync<StudentModel, Student>(s => s.LastName == "Change");

            //assert
            Assert.True(result);
            Assert.Equal(2, savedStudents.Count);
        }

        [Fact]
        public async Task AddGraphChange_AndSaveChangesAsync_PersistsGraph()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();
            var instructor = (await repository.GetAsync<InstructorModel, Instructor>
            (
                s => s.FullName == "Candace Kapoor",
                null,
                new SelectExpandDefinition
                (
                    [],
                    [
                        new SelectExpandItem(nameof(InstructorModel.OfficeAssignment))
                    ]
                )
            )).First();
            int id = instructor.ID;
            instructor.FirstName = "GraphChange";
            instructor.EntityState = Domain.EntityStateType.Modified;

            //act
            repository.AddGraphChange<InstructorModel, Instructor>(instructor);
            bool result = await repository.SaveChangesAsync();
            var savedInstructor = (await repository.GetAsync<InstructorModel, Instructor>(f => f.ID == id)).First();

            //assert
            Assert.True(result);
            Assert.Equal("GraphChange", savedInstructor.FirstName);
        }

        [Fact]
        public async Task AddGraphChanges_Collection_AndSaveChangesAsync_PersistsAllGraphs()
        {
            //arrange
            ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();
            var instructors = await repository.GetAsync<InstructorModel, Instructor>();

            foreach (var instructor in instructors)
            {
                instructor.FirstName += "_Graph";
                instructor.EntityState = Domain.EntityStateType.Modified;
            }

            //act
            repository.AddGraphChanges<InstructorModel, Instructor>(instructors);
            bool result = await repository.SaveChangesAsync();

            //assert
            Assert.True(result);
        }

        [Fact]
        public async Task ClearChangeTracker_RemovesAllChanges()
        {
            //arrange
            SchoolContext context = serviceProvider.GetRequiredService<SchoolContext>();
            SchoolStore store = new(context);
            SchoolRepository repository = new(store, serviceProvider.GetRequiredService<IMapper>());
            var student = (await repository.GetAsync<StudentModel, Student>(s => s.FullName == "Carson Alexander")).Single();
            student.FirstName = "ChangedName";
            student.EntityState = Domain.EntityStateType.Modified;

            repository.AddChange<StudentModel, Student>(student);
            Assert.NotEmpty(context.ChangeTracker.Entries());

            //act
            repository.ClearChangeTracker();
            Assert.Empty(context.ChangeTracker.Entries());
        }

        [Fact]
        public async Task DetachAllEntriesWorks()
        {
            //arrange
            SchoolContext context = serviceProvider.GetRequiredService<SchoolContext>();
            SchoolStore store = new(context);
            SchoolRepository repository = new(store, serviceProvider.GetRequiredService<IMapper>());
            var student = (await repository.GetAsync<StudentModel, Student>(s => s.FullName == "Carson Alexander")).Single();
            student.FirstName = "ChangedName";
            student.EntityState = Domain.EntityStateType.Modified;

            repository.AddChange<StudentModel, Student>(student);
            Assert.NotEmpty(context.ChangeTracker.Entries());

            //act
            repository.DetachAllEntries();
            Assert.Empty(context.ChangeTracker.Entries());
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
                        @"Server=(localdb)\mssqllocaldb;Database=RepositoryTest;ConnectRetryCount=0",
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

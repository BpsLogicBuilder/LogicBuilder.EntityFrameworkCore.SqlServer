using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using LogicBuilder.Data;
using LogicBuilder.EntityFrameworkCore.SqlServer.Crud;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.AutoMapperProfiles;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data.Stores;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Crud
{
    public class HelpersTest
    {
        static HelpersTest()
        {
            InitializeMapperConfiguration();
        }

        public HelpersTest()
        {
            Initialize();
        }

        #region Fields
        private IServiceProvider serviceProvider;
        #endregion Fields

        [Fact]
        public async Task ApplyStateChanges_WithAddedEntity_SetsEntityStateToAdded()
        {
            //arrange
            SchoolContext context = serviceProvider.GetRequiredService<SchoolContext>();
            var student = new Student
            {
                FirstName = "TestApply",
                LastName = "Added",
                EnrollmentDate = DateTime.UtcNow,
                EntityState = EntityStateType.Added
            };
            context.Add(student);

            //act
            context.ApplyStateChanges();

            //assert
            var entry = context.Entry(student);
            Assert.Equal(EntityState.Added, entry.State);
        }

        [Fact]
        public async Task ApplyStateChanges_WithModifiedEntity_SetsEntityStateToModified()
        {
            //arrange
            SchoolContext context = serviceProvider.GetRequiredService<SchoolContext>();
            ISchoolStore repository = serviceProvider.GetRequiredService<ISchoolStore>();
            var student = (await repository.GetAsync<Student>(s => s.LastName == "Alexander")).First();

            student.FirstName = "Modified";
            student.EntityState = EntityStateType.Modified;
            context.Attach(student);

            //act
            context.ApplyStateChanges();

            //assert
            var entry = context.Entry(student);
            Assert.Equal(EntityState.Modified, entry.State);
        }

        [Fact]
        public async Task ApplyStateChanges_WithDeletedEntity_SetsEntityStateToDeleted()
        {
            //arrange
            SchoolContext context = serviceProvider.GetRequiredService<SchoolContext>();
            ISchoolStore repository = serviceProvider.GetRequiredService<ISchoolStore>();
            var student = (await repository.GetAsync<Student>(s => s.LastName == "Alexander")).First();

            student.EntityState = EntityStateType.Deleted;
            context.Attach(student);

            //act
            context.ApplyStateChanges();

            //assert
            var entry = context.Entry(student);
            Assert.Equal(EntityState.Deleted, entry.State);
        }

        [Fact]
        public async Task ApplyStateChanges_WithUnchangedEntity_SetsEntityStateToUnchanged()
        {
            //arrange
            SchoolContext context = serviceProvider.GetRequiredService<SchoolContext>();
            ISchoolStore repository = serviceProvider.GetRequiredService<ISchoolStore>();
            var student = (await repository.GetAsync<Student>(s => s.LastName == "Alexander")).First();

            student.EntityState = EntityStateType.Unchanged;
            context.Attach(student);

            //act
            context.ApplyStateChanges();

            //assert
            var entry = context.Entry(student);
            Assert.Equal(EntityState.Unchanged, entry.State);
        }

        [Fact]
        public void SetStates_SetsAllEntitiesToSpecifiedState()
        {
            //arrange
            SchoolContext context = serviceProvider.GetRequiredService<SchoolContext>();
            var student1 = new Student
            {
                FirstName = "Student1",
                LastName = "Test",
                EnrollmentDate = DateTime.UtcNow,
                EntityState = EntityStateType.Added
            };
            var student2 = new Student
            {
                FirstName = "Student2",
                LastName = "Test",
                EnrollmentDate = DateTime.UtcNow,
                EntityState = EntityStateType.Added
            };
            context.Add(student1);
            context.Add(student2);

            //act
            context.SetStates(EntityState.Detached);

            //assert
            Assert.Equal(EntityState.Detached, context.Entry(student1).State);
            Assert.Equal(EntityState.Detached, context.Entry(student2).State);
        }

        [Fact]
        public async Task ConvertState_WithAddedState_ReturnsEntityStateAdded()
        {
            //arrange
            SchoolContext context = serviceProvider.GetRequiredService<SchoolContext>();
            var student = new Student
            {
                FirstName = "Convert",
                LastName = "Test",
                EnrollmentDate = DateTime.UtcNow,
                EntityState = EntityStateType.Added
            };
            context.Add(student);

            var entry = context.Entry((IBaseData)student);

            //act
            var result = Helpers.ConvertState(entry);

            //assert
            Assert.Equal(EntityState.Added, result);
        }

        [Fact]
        public async Task ConvertState_WithModifiedState_ReturnsEntityStateModified()
        {
            //arrange
            SchoolContext context = serviceProvider.GetRequiredService<SchoolContext>();
            ISchoolStore repository = serviceProvider.GetRequiredService<ISchoolStore>();
            var student = (await repository.GetAsync<Student>(s => s.LastName == "Alexander")).First();

            student.EntityState = EntityStateType.Modified;
            context.Attach(student);
            var entry = context.Entry((IBaseData)student);

            //act
            var result = entry.ConvertState();

            //assert
            Assert.Equal(EntityState.Modified, result);
        }

        [Fact]
        public async Task ConvertState_WithDeletedState_ReturnsEntityStateDeleted()
        {
            //arrange
            SchoolContext context = serviceProvider.GetRequiredService<SchoolContext>();
            ISchoolStore repository = serviceProvider.GetRequiredService<ISchoolStore>();
            var student = (await repository.GetAsync<Student>(s => s.LastName == "Alexander")).First();

            student.EntityState = EntityStateType.Deleted;
            context.Attach(student);
            var entry = context.Entry((IBaseData)student);

            //act
            var result = entry.ConvertState();

            //assert
            Assert.Equal(EntityState.Deleted, result);
        }

        [Fact]
        public async Task ConvertState_WithUnchangedState_ReturnsEntityStateUnchanged()
        {
            //arrange
            SchoolContext context = serviceProvider.GetRequiredService<SchoolContext>();
            ISchoolStore repository = serviceProvider.GetRequiredService<ISchoolStore>();
            var student = (await repository.GetAsync<Student>(s => s.LastName == "Alexander")).First();

            student.EntityState = EntityStateType.Unchanged;
            context.Attach(student);
            var entry = context.Entry((IBaseData)student);

            //act
            var result = entry.ConvertState();

            //assert
            Assert.Equal(EntityState.Unchanged, result);
        }

        [Fact]
        public async Task ConvertState_WithInvalidtate_Throws()
        {
            //arrange
            SchoolContext context = serviceProvider.GetRequiredService<SchoolContext>();
            ISchoolStore repository = serviceProvider.GetRequiredService<ISchoolStore>();
            var student = (await repository.GetAsync<Student>(s => s.LastName == "Alexander")).First();

            student.EntityState = (EntityStateType)999; // Invalid state
            context.Attach(student);
            var entry = context.Entry((IBaseData)student);

            //act & assert
            Assert.Throws<ArgumentException>(() => entry.ConvertState());
        }

        [Fact]
        public async Task DetachMatchingKeyEntries_DetachesEntitiesWithMatchingKeys()
        {
            //arrange
            SchoolContext context = serviceProvider.GetRequiredService<SchoolContext>();
            SchoolStore store = new(context);

            // Get a student from the database (this will be tracked)
            var trackedStudent = (await store.GetAsync<Student>(s => s.LastName == "Alexander")).First();
            trackedStudent.FirstName = "Updated";
            trackedStudent.EntityState = EntityStateType.Modified;
            store.AddChanges([trackedStudent]);
            int studentId = trackedStudent.ID;

            // Verify the student is tracked
            Assert.Contains(context.ChangeTracker.Entries<Student>(), e => e.Entity.ID == studentId);

            // Create a new student object with the same key
            var newStudent = new Student
            {
                ID = studentId,
                FirstName = "Updated",
                LastName = "Alexander",
                EnrollmentDate = DateTime.UtcNow,
                EntityState = EntityStateType.Modified
            };

            //act
            context.DetachMatchingKeyEntries(newStudent);

            //assert
            // The tracked entity should now be detached
            var entries = context.ChangeTracker.Entries<Student>().Where(e => e.Entity.ID == studentId);
            Assert.Empty(entries);
        }

        #region Helpers

        private static void InitializeMapperConfiguration()
        {
            MapperConfiguration ??= ConfigurationHelper.GetMapperConfiguration(cfg =>
            {
                cfg.AddExpressionMapping();

                cfg.AddProfile<SchoolProfile>();
                cfg.AddProfile<DataClassesMappings>();
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
                        @"Server=(localdb)\mssqllocaldb;Database=HelpersRepositoryTest;ConnectRetryCount=0",
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

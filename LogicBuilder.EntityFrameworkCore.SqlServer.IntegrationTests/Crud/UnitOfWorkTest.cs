using AutoMapper;
using AutoMapper.Extensions.ExpressionMapping;
using LogicBuilder.EntityFrameworkCore.SqlServer.Crud;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.AutoMapperProfiles;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data.Stores;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Crud
{
    public class UnitOfWorkTest
    {
        static UnitOfWorkTest()
        {
            InitializeMapperConfiguration();
        }

        public UnitOfWorkTest()
        {
            Initialize();
        }

        #region Fields
        private IServiceProvider serviceProvider;
        #endregion Fields

        [Fact]
        public async Task DiposingUnitOfWork_DisposesContext()
        {
            // Arrange
            SchoolContext context = serviceProvider.GetRequiredService<SchoolContext>();
            UnitOfWork unitOfWork = new(context);
            ICollection<Student> students = await unitOfWork.GetRepository<Student>().GetAsync();
            Assert.NotEmpty(students);

            // Act
            unitOfWork.Dispose();

            // Assert
            Assert.Throws<ObjectDisposedException>(() => context.Students.ToList());
        }

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
                        @"Server=(localdb)\mssqllocaldb;Database=UnitOfWorkTest;ConnectRetryCount=0",
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
    }
}

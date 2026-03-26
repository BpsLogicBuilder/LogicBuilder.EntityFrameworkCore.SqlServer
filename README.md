# LogicBuilder.EntityFrameworkCore.SqlServer

[![CI](https://github.com/BpsLogicBuilder/LogicBuilder.EntityFrameworkCore.SqlServer/actions/workflows/ci.yml/badge.svg)](https://github.com/BpsLogicBuilder/LogicBuilder.EntityFrameworkCore.SqlServer/actions/workflows/ci.yml)
[![CodeQL](https://github.com/BpsLogicBuilder/LogicBuilder.EntityFrameworkCore.SqlServer/actions/workflows/github-code-scanning/codeql/badge.svg)](https://github.com/BpsLogicBuilder/LogicBuilder.EntityFrameworkCore.SqlServer/actions/workflows/github-code-scanning/codeql)
[![codecov](https://codecov.io/gh/BpsLogicBuilder/LogicBuilder.EntityFrameworkCore.SqlServer/graph/badge.svg?token=7Y55F6XE9W)](https://codecov.io/gh/BpsLogicBuilder/LogicBuilder.EntityFrameworkCore.SqlServer)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=BpsLogicBuilder_LogicBuilder.EntityFrameworkCore.SqlServer&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=BpsLogicBuilder_LogicBuilder.EntityFrameworkCore.SqlServer)
[![NuGet](https://img.shields.io/nuget/v/LogicBuilder.EntityFrameworkCore.SqlServer.svg)](https://www.nuget.org/packages/LogicBuilder.EntityFrameworkCore.SqlServer)

A repository pattern library for Entity Framework Core that provides seamless mapping between business models and data entities, with advanced expression translation capabilities.

## Features

- **Two-Way Object Mapping**: Automatically maps business model objects to and from data entities using AutoMapper
- **Expression Translation**: Translates LINQ query expressions from business model to data model, enabling queries to be written against your business layer
- **Repository Pattern**: Implements a clean repository pattern with `IContextRepository` and `ContextRepositoryBase`
- **Store Layer**: Provides `IStore` and `StoreBase` for direct DbContext operations
- **Advanced Querying**: Supports complex operations including:
  - Filtering with `Expression<Func<TModel, bool>>`
  - Sorting and pagination
  - Grouping and aggregations
  - Select/expand operations
  - Include navigation properties
- **CRUD Operations**: Full support for Create, Read, Update, Delete operations
- **Graph Operations**: Save entire object graphs with `SaveGraphAsync` and `SaveGraphsAsync`
- **Change Tracking**: Methods to manage EF Core change tracker (`AddChanges`, `ClearChangeTracker`, `DetachAllEntries`)
- **Batch Operations**: Support for batch saves and deletes

## Installation

```
dotnet add package LogicBuilder.EntityFrameworkCore.SqlServer
```

## Usage
```c#
    //Create a context
    public class SchoolContext : DbContext
    {
        public SchoolContext(DbContextOptions<SchoolContext> options) : base(options)
        {
        }
    }

    //Create Store
    public interface ISchoolStore : IStore
    {
    }
    public class SchoolStore : StoreBase, ISchoolStore
    {
        public SchoolStore(SchoolContext context)
            : base(context)
        {
        }
    }

    //Create Repository
    public interface ISchoolRepository : IContextRepository
    {
    }
    public class SchoolRepository : ContextRepositoryBase, ISchoolRepository
    {
        public SchoolRepository(ISchoolStore store, IMapper mapper) : base(store, mapper)
        {
        }
    }

    //Register Services including AutoMapper profiles.
    IServiceProvider serviceProvider = new ServiceCollection()
                .AddDbContext<SchoolContext>
                (
                    options =>
                    {
                        options.UseInMemoryDatabase("ContosoUniversity");
                        options.UseInternalServiceProvider(new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider());
                    }
                )
                .AddTransient<ISchoolStore, SchoolStore>()
                .AddTransient<ISchoolRepository, SchoolRepository>()
                .AddSingleton<AutoMapper.IConfigurationProvider>(new MapperConfiguration(cfg => cfg.AddProfiles(typeof(SchoolProfile).GetTypeInfo().Assembly)))
                .AddTransient<IMapper>(sp => new Mapper(sp.GetRequiredService<AutoMapper.IConfigurationProvider>(), sp.GetService))
                .BuildServiceProvider();

    //Call the repository (inside an async method)
    ISchoolRepository repository = serviceProvider.GetRequiredService<ISchoolRepository>();
    ICollection<StudentModel> list = await repository.GetAsync<StudentModel, Student>();
```


## Key Interfaces

### IContextRepository

Provides methods for:
- `GetAsync<TModel, TData>` - Retrieve entities with optional filtering and querying
- `CountAsync<TModel, TData>` - Count entities matching a filter
- `QueryAsync<TModel, TData, TModelReturn, TDataReturn>` - Execute complex queries with expression translation
- `SaveAsync<TModel, TData>` - Save single or multiple entities
- `SaveGraphAsync<TModel, TData>` - Save entity graphs
- `DeleteAsync<TModel, TData>` - Delete entities by filter
- `AddChanges<TModel, TData>` - Stage changes without saving
- `SaveChangesAsync` - Persist all staged changes
- `ClearChangeTracker` / `DetachAllEntries` - Manage change tracking

## Requirements

- .NET 8.0 or higher
- Entity Framework Core
- AutoMapper with expression mapping support

using AutoMapper;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data.Stores;
using LogicBuilder.EntityFrameworkCore.SqlServer.Repositories;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models.Repositories
{
    public class SchoolRepository(ISchoolStore store, IMapper mapper) : ContextRepositoryBase(store, mapper), ISchoolRepository
    {
    }
}

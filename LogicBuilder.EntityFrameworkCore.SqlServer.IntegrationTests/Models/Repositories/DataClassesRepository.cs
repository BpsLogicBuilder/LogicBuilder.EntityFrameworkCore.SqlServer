using AutoMapper;
using LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data.Stores;
using LogicBuilder.EntityFrameworkCore.SqlServer.Repositories;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models.Repositories
{
    public class DataClassesRepository(IDataClassesStore store, IMapper mapper) : ContextRepositoryBase(store, mapper), IDataClassesRepository
    {
    }
}

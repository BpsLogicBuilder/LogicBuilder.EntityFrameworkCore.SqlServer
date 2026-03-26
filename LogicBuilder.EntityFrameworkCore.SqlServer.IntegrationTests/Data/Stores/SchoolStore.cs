using LogicBuilder.EntityFrameworkCore.SqlServer.Crud.DataStores;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data.Stores
{
    public class SchoolStore(SchoolContext context) : StoreBase(context), ISchoolStore
    {
    }
}

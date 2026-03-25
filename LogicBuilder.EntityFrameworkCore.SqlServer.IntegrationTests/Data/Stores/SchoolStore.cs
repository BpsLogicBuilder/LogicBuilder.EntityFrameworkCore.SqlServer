using LogicBuilder.EntityFrameworkCore.SqlServer.Crud.DataStores;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data.Stores
{
    public class SchoolStore : StoreBase, ISchoolStore
    {
        public SchoolStore(SchoolContext context)
            : base(context)
        {
        }
    }
}

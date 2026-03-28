using LogicBuilder.EntityFrameworkCore.SqlServer.Crud.DataStores;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data.Stores
{
    public class DataClassesStore(DataClassesContext context) : StoreBase(context), IDataClassesStore
    {
    }
}

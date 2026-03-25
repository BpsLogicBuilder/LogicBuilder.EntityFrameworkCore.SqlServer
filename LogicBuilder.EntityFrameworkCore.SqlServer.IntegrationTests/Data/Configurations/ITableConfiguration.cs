using Microsoft.EntityFrameworkCore;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data.Configurations
{
    interface ITableConfiguration
    {
        void Configure(ModelBuilder modelBuilder);
    }
}

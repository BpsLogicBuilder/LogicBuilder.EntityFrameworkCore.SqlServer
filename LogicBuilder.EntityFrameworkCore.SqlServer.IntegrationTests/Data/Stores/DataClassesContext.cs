using LogicBuilder.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Reflection;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data.Stores
{
    public class DataClassesContext(DbContextOptions<DataClassesContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<DataTypes> DataTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (PropertyInfo property in this.GetType().GetProperties())
            {
                if (property.PropertyType.Name != "DbSet`1")
                    continue;

                Type modelType = property.PropertyType.GetGenericArguments()[0];
                if (!typeof(BaseData).IsAssignableFrom(modelType))
                    continue;

                modelBuilder.Entity(modelType).Ignore(nameof(BaseData.EntityState));
            }

            base.OnModelCreating(modelBuilder);
        }
    }
}

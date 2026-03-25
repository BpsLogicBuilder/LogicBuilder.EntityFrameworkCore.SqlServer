using Microsoft.EntityFrameworkCore;

namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Data.Configurations
{
    class CourseAssignmentConfiguration : ITableConfiguration
    {
        public void Configure(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CourseAssignment>()
                .HasKey(c => new { c.CourseID, c.InstructorID });
        }
    }
}

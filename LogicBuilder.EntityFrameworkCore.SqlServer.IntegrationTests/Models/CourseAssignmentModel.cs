namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models
{
    public class CourseAssignmentModel : BaseModelClass
    {
		public int InstructorID { get; set; }

		public int CourseID { get; set; }

        public string CourseTitle { get; set; }

        public string CourseNumberAndTitle { get; set; }

        public string Department { get; set; }
    }
}
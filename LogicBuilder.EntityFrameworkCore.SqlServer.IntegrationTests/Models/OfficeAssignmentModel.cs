using System.ComponentModel.DataAnnotations;


namespace LogicBuilder.EntityFrameworkCore.SqlServer.IntegrationTests.Models
{
    public class OfficeAssignmentModel : BaseModelClass
    {
		public int InstructorID { get; set; }

		[StringLength(50)]
		[Display(Name = "Office Location")]
		public string Location { get; set; }
    }
}
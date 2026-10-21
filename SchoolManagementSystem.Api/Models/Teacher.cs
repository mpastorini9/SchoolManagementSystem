namespace SchoolManagementSystem.Api.Models
{
    public class Teacher : ISchoolScoped
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string DocumentNumber { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        public ApplicationUser? User { get; set; }
    }
}

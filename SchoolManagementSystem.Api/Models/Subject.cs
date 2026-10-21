namespace SchoolManagementSystem.Api.Models
{
    public class Subject : ISchoolScoped
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}

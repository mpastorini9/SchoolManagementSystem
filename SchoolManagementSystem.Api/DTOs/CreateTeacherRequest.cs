namespace SchoolManagementSystem.Api.DTOs
{
    public class CreateTeacherRequest
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
    }
}

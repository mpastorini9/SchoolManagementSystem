namespace SchoolManagementSystem.Api.DTOs
{
    public class CreateStudentSubmissionRequest
    {
        public string? Value { get; set; }

        public IFormFile? File { get; set; }
    }
}

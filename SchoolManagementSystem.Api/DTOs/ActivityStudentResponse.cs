namespace SchoolManagementSystem.Api.DTOs
{
    public class ActivityStudentResponse
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public DateTime? ExtendedDueAt { get; set; }

        public DateTime AssignedAt { get; set; }
    }
}
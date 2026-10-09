namespace SchoolManagementSystem.Api.DTOs
{
    public class StudentSubmissionResponse
    {
        public int Id { get; set; }

        public int ActivityId { get; set; }

        public int StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; }

        public DateTime? ReceivedAt { get; set; }

        public string? Value { get; set; }

        public string? TeacherObservation { get; set; }

        public string SubmissionStatus { get; set; } = string.Empty;

        public SubmissionRevisionResponse? CurrentRevision { get; set; }
    }
}
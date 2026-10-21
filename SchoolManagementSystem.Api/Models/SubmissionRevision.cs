namespace SchoolManagementSystem.Api.Models
{
    public class SubmissionRevision : ISchoolScoped
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }

        public int StudentSubmissionId { get; set; }
        public StudentSubmission? StudentSubmission { get; set; }

        public int Version { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string? Value { get; set; }

        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        public string? StorageKey { get; set; }

        public DateTime SubmittedAt { get; set; }

        public bool IsCurrent { get; set; }

        public bool WasAuthorizedCorrection { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

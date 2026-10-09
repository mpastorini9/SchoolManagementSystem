namespace SchoolManagementSystem.Api.DTOs
{
    public class SubmissionRevisionResponse
    {
        public int Id { get; set; }

        public int Version { get; set; }

        public string FileName { get; set; } = string.Empty;

        public string? ContentType { get; set; }

        public long FileSize { get; set; }

        public DateTime SubmittedAt { get; set; }

        public bool IsCurrent { get; set; }

        public bool WasAuthorizedCorrection { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
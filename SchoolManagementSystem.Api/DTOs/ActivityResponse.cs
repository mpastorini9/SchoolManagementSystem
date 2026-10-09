namespace SchoolManagementSystem.Api.DTOs
{
    public class ActivityResponse
    {
        public int Id { get; set; }

        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;

        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;

        public int SubjectId { get; set; }
        public string SubjectName { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime DueAt { get; set; }

        public DateTime? ExtendedDueAt { get; set; }

        public int TargetType { get; set; }

        public int? TargetStudentId { get; set; }
        public string? TargetStudentName { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? PublishedAt { get; set; }

        public bool IsActive { get; set; }
    }
}
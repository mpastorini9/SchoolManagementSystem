namespace SchoolManagementSystem.Api.Models
{
    public class Activity
    {
        public int Id { get; set; }

        public int TeacherId { get; set; }
        public Teacher? Teacher { get; set; }

        public int CourseId { get; set; }
        public Course? Course { get; set; }

        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime DueAt { get; set; }

        public DateTime? ExtendedDueAt { get; set; }

        public ActivityTargetType TargetType { get; set; }

        public int? TargetStudentId { get; set; }
        public Student? TargetStudent { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? PublishedAt { get; set; }

        public bool IsActive { get; set; } = true;
    }

    public enum ActivityTargetType
    {
        Course = 1,
        Student = 2
    }
}
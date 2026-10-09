using SchoolManagementSystem.Api.Models;

namespace SchoolManagementSystem.Api.DTOs
{
    public class CreateActivityRequest
    {
        public int TeacherId { get; set; }

        public int CourseId { get; set; }

        public int SubjectId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime DueAt { get; set; }

        public ActivityTargetType TargetType { get; set; }

        public int? TargetStudentId { get; set; }
    }
}
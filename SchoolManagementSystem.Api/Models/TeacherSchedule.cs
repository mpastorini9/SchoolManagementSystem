namespace SchoolManagementSystem.Api.Models
{
    public class TeacherSchedule
    {
        public int Id { get; set; }
        public int TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        public int SubjectId { get; set; }
        public Subject? Subject { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}

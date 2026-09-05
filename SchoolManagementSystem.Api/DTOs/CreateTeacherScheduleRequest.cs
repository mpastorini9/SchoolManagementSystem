namespace SchoolManagementSystem.Api.DTOs
{
    public class CreateTeacherScheduleRequest
    {
        public int TeacherId { get; set; }
        public int CourseId { get; set; }
        public int SubjectId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }
}

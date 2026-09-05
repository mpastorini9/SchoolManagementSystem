using SchoolManagementSystem.Api.Models;

namespace SchoolManagementSystem.Api.DTOs
{
    public class TeacherAttendanceResponse
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public int TeacherId { get; set; }
        public string TeacherName { get; set; } = string.Empty;
        public string CourseName { get; set; } = string.Empty;
        public string SubjectName { get; set; } = string.Empty;
        public decimal HoursTaught { get; set; }
        public TeacherAttendanceStatus Status { get; set; }
    }
}

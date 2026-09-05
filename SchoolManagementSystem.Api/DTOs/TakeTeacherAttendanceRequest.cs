using SchoolManagementSystem.Api.Models;

namespace SchoolManagementSystem.Api.DTOs
{
    public class TakeTeacherAttendanceRequest
    {
        public int TeacherScheduleId { get; set; }
        public DateTime Date { get; set; }
        public TeacherAttendanceStatus Status { get; set; }
    }
}

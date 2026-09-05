namespace SchoolManagementSystem.Api.Models
{
    public enum TeacherAttendanceStatus
    {
        Present,
        Absent
    }

    public class TeacherAttendance
    {
        public int Id { get; set; }
        public int TeacherScheduleId { get; set; }
        public TeacherSchedule? TeacherSchedule { get; set; }
        public DateTime Date { get; set; }
        public TeacherAttendanceStatus Status { get; set; }
    }
}

namespace SchoolManagementSystem.Api.Models
{
    public enum AttendanceStatus
    {
        Present,
        Absent,
        Late,
        Justified
    }
    public class Attendance
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student? Student { get; set; }
        public DateTime Date { get; set; }
        public AttendanceStatus Status { get; set; }
        public string? Observation { get; set; }
    }
}
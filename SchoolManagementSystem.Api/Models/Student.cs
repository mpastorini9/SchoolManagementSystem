namespace SchoolManagementSystem.Api.Models
{
    using System.Collections.Generic;
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string DocumentNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public int CourseId { get; set; }
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public Course Course { get; set; }

    }
}
namespace SchoolManagementSystem.Api.Models
{
    using System.Collections.Generic;
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DocumentNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public int CourseId { get; set; }
        public ICollection<Attendance> Attendances { get; set; }
        public Course Course { get; set; }

    }
}
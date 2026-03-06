namespace SchoolManagementSystem.Api.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DocumentNumber { get; set; }

        public DateTime DateOfBirth { get; set; }
        public int CourseId { get; set; }
    }
}
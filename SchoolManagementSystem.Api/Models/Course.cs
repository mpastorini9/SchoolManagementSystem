namespace SchoolManagementSystem.Api.Models

{
    using System.Collections.Generic;
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
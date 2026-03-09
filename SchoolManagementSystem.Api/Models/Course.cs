namespace SchoolManagementSystem.Api.Models

{
    using System.Collections.Generic;
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Student> Students { get; set; }
    }
}
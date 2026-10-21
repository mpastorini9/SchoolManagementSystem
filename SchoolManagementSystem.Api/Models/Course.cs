/*namespace SchoolManagementSystem.Api.Models

{
    using System.Collections.Generic;
    public class Course : ISchoolScoped
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}*/
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SchoolManagementSystem.Api.Models
{
    public class Course : ISchoolScoped
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }

        public string Name { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}

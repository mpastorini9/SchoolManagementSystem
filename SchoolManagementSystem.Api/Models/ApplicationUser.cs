using Microsoft.AspNetCore.Identity;

namespace SchoolManagementSystem.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int SchoolId { get; set; }
        public School? School { get; set; }
        public int? StudentId { get; set; }
        public Student? Student { get; set; }

        public int? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }

        public ICollection<FamilyStudentLink> FamilyStudentLinks { get; set; } = new List<FamilyStudentLink>();
    }
}

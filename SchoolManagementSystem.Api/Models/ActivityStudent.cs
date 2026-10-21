namespace SchoolManagementSystem.Api.Models
{
    public class ActivityStudent : ISchoolScoped
    {
        public int Id { get; set; }
        public int SchoolId { get; set; }

        public int ActivityId { get; set; }
        public Activity? Activity { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public DateTime? ExtendedDueAt { get; set; }

        public DateTime AssignedAt { get; set; }
    }
}

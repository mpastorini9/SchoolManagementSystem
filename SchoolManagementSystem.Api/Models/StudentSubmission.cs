namespace SchoolManagementSystem.Api.Models
{
    using System.Collections.Generic;

    public class StudentSubmission
    {
        public int Id { get; set; }

        public int ActivityStudentId { get; set; }
        public ActivityStudent? ActivityStudent { get; set; }

        public DateTime SubmittedAt { get; set; }

        public DateTime? ReceivedAt { get; set; }

        public string? Value { get; set; }

        public string? TeacherObservation { get; set; }

        public ICollection<SubmissionRevision> Revisions { get; set; } = new List<SubmissionRevision>();
    }
}

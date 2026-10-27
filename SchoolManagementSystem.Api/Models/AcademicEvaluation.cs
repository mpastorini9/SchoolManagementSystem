namespace SchoolManagementSystem.Api.Models;

public sealed class AcademicEvaluation : ISchoolScoped
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public int CourseId { get; set; }
    public Course? Course { get; set; }
    public int SubjectId { get; set; }
    public Subject? Subject { get; set; }
    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime EvaluatedAt { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public ICollection<StudentGrade> Grades { get; set; } = new List<StudentGrade>();
}

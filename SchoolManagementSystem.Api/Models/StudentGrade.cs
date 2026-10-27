namespace SchoolManagementSystem.Api.Models;

public sealed class StudentGrade : ISchoolScoped
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public int EvaluationId { get; set; }
    public AcademicEvaluation? Evaluation { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public string Value { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUserId { get; set; }
    public DateTime? PublishedAt { get; set; }
}

namespace SchoolManagementSystem.Api.Models;

public sealed class StudentRecordEntry : ISchoolScoped
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public StudentRecordCategory Category { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByUserId { get; set; } = string.Empty;
    public bool IsVisibleToFamily { get; set; }
}

public enum StudentRecordCategory
{
    Observation = 1,
    Incident = 2,
    Recognition = 3,
    ConfidentialNote = 4
}

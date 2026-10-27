namespace SchoolManagementSystem.Api.Models;

public sealed class FamilyStudentLink : ISchoolScoped
{
    public int Id { get; set; }
    public int SchoolId { get; set; }
    public string FamilyUserId { get; set; } = string.Empty;
    public ApplicationUser? FamilyUser { get; set; }
    public int StudentId { get; set; }
    public Student? Student { get; set; }
    public DateTime LinkedAt { get; set; }
    public string LinkedByUserId { get; set; } = string.Empty;
}

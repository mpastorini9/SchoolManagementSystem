namespace SchoolManagementSystem.Api.Models;

public sealed class School
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public interface ISchoolScoped
{
    int SchoolId { get; set; }
}

using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.Models;
using SchoolManagementSystem.Api.Services;

namespace SchoolManagementSystem.Api.Controllers;

[ApiController]
[Route("api/admin/students/{studentId:int}/records")]
[Authorize(Roles = SchoolRoles.SchoolAdministrator)]
public sealed class StudentRecordManagementController : ControllerBase
{
    private readonly SchoolContext _context;
    public StudentRecordManagementController(SchoolContext context) => _context = context;

    [HttpPost]
    public async Task<IActionResult> Create(int studentId, CreateStudentRecordRequest request, CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirst("school_id")?.Value, out var schoolId) || schoolId <= 0) return Forbid();
        if (!Enum.IsDefined(request.Category) || string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 160 ||
            string.IsNullOrWhiteSpace(request.Description) || request.Description.Trim().Length > 4000 || request.RecordedAt == default)
            return BadRequest("La categoría, el título, el detalle y la fecha son obligatorios.");
        if (request.IsVisibleToFamily && request.Category == StudentRecordCategory.ConfidentialNote)
            return BadRequest("Las notas confidenciales no pueden habilitarse para consulta familiar.");
        if (!await _context.Students.AnyAsync(student => student.Id == studentId && student.SchoolId == schoolId, cancellationToken))
            return NotFound();

        var entry = new StudentRecordEntry
        {
            StudentId = studentId,
            Category = request.Category,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            RecordedAt = request.RecordedAt,
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = User.FindFirst("sub")?.Value ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty,
            IsVisibleToFamily = request.IsVisibleToFamily
        };
        _context.StudentRecordEntries.Add(entry);
        await _context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Create), new { studentId, id = entry.Id }, new
        {
            entry.Id, entry.StudentId, entry.Category, entry.Title, entry.RecordedAt, entry.IsVisibleToFamily
        });
    }
}

public sealed record CreateStudentRecordRequest(StudentRecordCategory Category, string Title, string Description, DateTime RecordedAt, bool IsVisibleToFamily = false);

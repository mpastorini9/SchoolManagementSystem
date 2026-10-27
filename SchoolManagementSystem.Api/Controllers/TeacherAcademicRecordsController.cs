using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.Models;
using SchoolManagementSystem.Api.Services;

namespace SchoolManagementSystem.Api.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = SchoolRoles.Teacher + "," + SchoolRoles.SchoolAdministrator)]
public sealed class TeacherAcademicRecordsController : ControllerBase
{
    private readonly SchoolContext _context;

    public TeacherAcademicRecordsController(SchoolContext context) => _context = context;

    [HttpPost("evaluations")]
    public async Task<IActionResult> CreateEvaluation(CreateAcademicEvaluationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 160 || request.EvaluatedAt == default)
            return BadRequest("Se requiere título y fecha de evaluación válidos.");
        if (!int.TryParse(User.FindFirst("school_id")?.Value, out var schoolId) || schoolId <= 0)
            return Forbid();

        var isAdministrator = User.IsInRole(SchoolRoles.SchoolAdministrator);
        var teacherId = request.TeacherId;
        if (!isAdministrator)
        {
            if (!int.TryParse(User.FindFirst("teacher_id")?.Value, out var currentTeacherId)) return Forbid();
            teacherId = currentTeacherId;
        }
        if (teacherId is null || teacherId <= 0) return BadRequest("Se requiere un docente responsable.");

        var assigned = await _context.TeacherSchedules.AnyAsync(schedule => schedule.SchoolId == schoolId &&
            schedule.TeacherId == teacherId && schedule.CourseId == request.CourseId && schedule.SubjectId == request.SubjectId &&
            schedule.Teacher!.IsActive, cancellationToken);
        if (!assigned) return NotFound();

        var evaluation = new AcademicEvaluation
        {
            CourseId = request.CourseId,
            SubjectId = request.SubjectId,
            TeacherId = teacherId.Value,
            Title = request.Title.Trim(),
            EvaluatedAt = request.EvaluatedAt,
            CreatedByUserId = CurrentUserId()
        };
        _context.AcademicEvaluations.Add(evaluation);
        await _context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(CreateEvaluation), new { id = evaluation.Id }, new
        {
            evaluation.Id, evaluation.CourseId, evaluation.SubjectId, evaluation.TeacherId,
            evaluation.Title, evaluation.EvaluatedAt
        });
    }

    [HttpPost("evaluations/{evaluationId:int}/grades")]
    public async Task<IActionResult> CreateGrade(int evaluationId, CreateStudentGradeRequest request, CancellationToken cancellationToken)
    {
        var evaluation = await _context.AcademicEvaluations.FirstOrDefaultAsync(item => item.Id == evaluationId, cancellationToken);
        if (evaluation is null) return NotFound();
        if (!await CanManageAsync(evaluation, cancellationToken)) return Forbid();
        var student = await _context.Students.FirstOrDefaultAsync(item => item.Id == request.StudentId &&
            item.SchoolId == evaluation.SchoolId && item.CourseId == evaluation.CourseId, cancellationToken);
        if (student is null) return NotFound();
        if (string.IsNullOrWhiteSpace(request.Value) || request.Value.Trim().Length > 200)
            return BadRequest("El resultado debe contener entre 1 y 200 caracteres.");
        if (await _context.StudentGrades.AnyAsync(grade => grade.EvaluationId == evaluationId && grade.StudentId == student.Id, cancellationToken))
            return Conflict("Ya existe un resultado para este alumno en la evaluación.");

        var grade = new StudentGrade
        {
            EvaluationId = evaluation.Id,
            StudentId = student.Id,
            Value = request.Value.Trim(),
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = CurrentUserId()
        };
        _context.StudentGrades.Add(grade);
        await _context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(CreateGrade), new { evaluationId, id = grade.Id }, new
        {
            grade.Id, grade.EvaluationId, grade.StudentId, grade.Value, grade.CreatedAt
        });
    }

    [HttpPut("grades/{gradeId:int}")]
    public async Task<IActionResult> UpdateGrade(int gradeId, UpdateStudentGradeRequest request, CancellationToken cancellationToken)
    {
        var grade = await _context.StudentGrades.Include(item => item.Evaluation)
            .FirstOrDefaultAsync(item => item.Id == gradeId, cancellationToken);
        if (grade?.Evaluation is null) return NotFound();
        if (!await CanManageAsync(grade.Evaluation, cancellationToken)) return Forbid();
        if (string.IsNullOrWhiteSpace(request.Value) || request.Value.Trim().Length > 200)
            return BadRequest("El resultado debe contener entre 1 y 200 caracteres.");
        grade.Value = request.Value.Trim();
        grade.UpdatedAt = DateTime.UtcNow;
        grade.UpdatedByUserId = CurrentUserId();
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("grades/{gradeId:int}/publish")]
    public async Task<IActionResult> PublishGrade(int gradeId, CancellationToken cancellationToken)
    {
        var grade = await _context.StudentGrades.Include(item => item.Evaluation)
            .FirstOrDefaultAsync(item => item.Id == gradeId, cancellationToken);
        if (grade?.Evaluation is null) return NotFound();
        if (!await CanManageAsync(grade.Evaluation, cancellationToken)) return Forbid();
        if (grade.PublishedAt.HasValue) return NoContent();
        grade.PublishedAt = DateTime.UtcNow;
        grade.UpdatedAt = DateTime.UtcNow;
        grade.UpdatedByUserId = CurrentUserId();
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<bool> CanManageAsync(AcademicEvaluation evaluation, CancellationToken cancellationToken)
    {
        if (User.IsInRole(SchoolRoles.SchoolAdministrator)) return true;
        return int.TryParse(User.FindFirst("teacher_id")?.Value, out var teacherId) &&
            teacherId == evaluation.TeacherId && await _context.Teachers.AnyAsync(teacher =>
                teacher.Id == teacherId && teacher.IsActive && teacher.SchoolId == evaluation.SchoolId, cancellationToken);
    }

    private string CurrentUserId() => User.FindFirst("sub")?.Value ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
}

public sealed record CreateAcademicEvaluationRequest(int CourseId, int SubjectId, string Title, DateTime EvaluatedAt, int? TeacherId = null);
public sealed record CreateStudentGradeRequest(int StudentId, string Value);
public sealed record UpdateStudentGradeRequest(string Value);

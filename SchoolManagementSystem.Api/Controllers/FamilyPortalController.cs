using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.Models;
using SchoolManagementSystem.Api.Services;

namespace SchoolManagementSystem.Api.Controllers;

[ApiController]
[Route("api/family")]
[Authorize(Roles = SchoolRoles.Family)]
public sealed class FamilyPortalController : ControllerBase
{
    private readonly SchoolContext _context;
    private readonly ICurrentSchoolContext _currentSchool;
    private readonly ActivityFileStorage _fileStorage;

    public FamilyPortalController(
        SchoolContext context,
        ICurrentSchoolContext currentSchool,
        ActivityFileStorage fileStorage)
    {
        _context = context;
        _currentSchool = currentSchool;
        _fileStorage = fileStorage;
    }

    [HttpGet("students")]
    public async Task<IActionResult> GetStudents(CancellationToken cancellationToken)
    {
        if (!TryGetFamilyIdentity(out var familyUserId, out var schoolId)) return Forbid();
        var students = await _context.FamilyStudentLinks
            .Where(link => link.FamilyUserId == familyUserId && link.SchoolId == schoolId && link.Student!.SchoolId == schoolId)
            .OrderBy(link => link.Student!.LastName)
            .ThenBy(link => link.Student!.FirstName)
            .Select(link => new FamilyStudentResponse(
                link.StudentId,
                link.Student!.FirstName,
                link.Student.LastName,
                link.Student.Course!.Name))
            .ToListAsync(cancellationToken);
        return Ok(students);
    }

    [HttpGet("students/{studentId:int}/attendance")]
    public async Task<IActionResult> GetAttendance(int studentId, CancellationToken cancellationToken)
    {
        if (!TryGetFamilyIdentity(out var familyUserId, out var schoolId)) return Forbid();
        if (!await IsLinkedStudentAsync(familyUserId, schoolId, studentId, cancellationToken)) return NotFound();

        var attendance = await _context.Attendances
            .Where(item => item.StudentId == studentId && item.SchoolId == schoolId)
            .OrderByDescending(item => item.Date)
            .ThenByDescending(item => item.Id)
            .Select(item => new FamilyAttendanceResponse(item.Id, item.Date, item.Status))
            .ToListAsync(cancellationToken);
        return Ok(attendance);
    }

    [HttpGet("students/{studentId:int}/schedule")]
    public async Task<IActionResult> GetSchedule(int studentId, CancellationToken cancellationToken)
    {
        if (!TryGetFamilyIdentity(out var familyUserId, out var schoolId)) return Forbid();
        if (!await IsLinkedStudentAsync(familyUserId, schoolId, studentId, cancellationToken)) return NotFound();

        var schedule = await _context.TeacherSchedules
            .Where(item => item.SchoolId == schoolId && item.CourseId == _context.Students
                .Where(student => student.Id == studentId && student.SchoolId == schoolId)
                .Select(student => student.CourseId).FirstOrDefault() && item.Teacher!.IsActive)
            .OrderBy(item => item.DayOfWeek).ThenBy(item => item.StartTime)
            .Select(item => new FamilyScheduleResponse(item.DayOfWeek, item.StartTime, item.EndTime,
                item.Subject!.Name, item.Teacher!.FirstName + " " + item.Teacher.LastName))
            .ToListAsync(cancellationToken);
        return Ok(schedule);
    }

    [HttpGet("students/{studentId:int}/grades")]
    public async Task<IActionResult> GetGrades(int studentId, CancellationToken cancellationToken)
    {
        if (!TryGetFamilyIdentity(out var familyUserId, out var schoolId)) return Forbid();
        if (!await IsLinkedStudentAsync(familyUserId, schoolId, studentId, cancellationToken)) return NotFound();
        var grades = await _context.StudentGrades
            .Where(grade => grade.StudentId == studentId && grade.SchoolId == schoolId && grade.PublishedAt.HasValue &&
                grade.Evaluation!.SchoolId == schoolId)
            .OrderByDescending(grade => grade.Evaluation!.EvaluatedAt).ThenByDescending(grade => grade.Id)
            .Select(grade => new FamilyGradeResponse(grade.Id, grade.Evaluation!.Title, grade.Value,
                grade.Evaluation.EvaluatedAt, grade.Evaluation.Subject!.Name, grade.Evaluation.Course!.Name,
                grade.Evaluation.Teacher!.FirstName + " " + grade.Evaluation.Teacher.LastName, grade.PublishedAt!.Value))
            .ToListAsync(cancellationToken);
        return Ok(grades);
    }

    [HttpGet("students/{studentId:int}/records")]
    public async Task<IActionResult> GetStudentRecords(int studentId, CancellationToken cancellationToken)
    {
        if (!TryGetFamilyIdentity(out var familyUserId, out var schoolId)) return Forbid();
        if (!await IsLinkedStudentAsync(familyUserId, schoolId, studentId, cancellationToken)) return NotFound();
        var records = await _context.StudentRecordEntries
            .Where(entry => entry.StudentId == studentId && entry.SchoolId == schoolId && entry.IsVisibleToFamily &&
                entry.Category != StudentRecordCategory.ConfidentialNote)
            .OrderByDescending(entry => entry.RecordedAt).ThenByDescending(entry => entry.Id)
            .Select(entry => new FamilyStudentRecordResponse(entry.Id, entry.Category, entry.Title, entry.Description, entry.RecordedAt))
            .ToListAsync(cancellationToken);
        return Ok(records);
    }

    [HttpGet("students/{studentId:int}/activities")]
    public async Task<IActionResult> GetActivities(int studentId, CancellationToken cancellationToken)
    {
        if (!TryGetFamilyIdentity(out var familyUserId, out var schoolId)) return Forbid();
        if (!await IsLinkedStudentAsync(familyUserId, schoolId, studentId, cancellationToken)) return NotFound();

        var assignedActivities = await _context.ActivityStudents
            .Where(assignment => assignment.StudentId == studentId && assignment.SchoolId == schoolId &&
                assignment.Activity!.SchoolId == schoolId && assignment.Activity.IsActive && assignment.Activity.PublishedAt.HasValue)
            .OrderByDescending(assignment => assignment.Activity!.PublishedAt)
            .ThenByDescending(assignment => assignment.ActivityId)
            .Select(assignment => new
            {
                assignment.Id,
                Activity = new FamilyActivityResponse(
                    assignment.ActivityId,
                    assignment.Activity!.Title,
                    assignment.Activity.Description,
                    assignment.Activity.Course!.Name,
                    assignment.Activity.Subject!.Name,
                    assignment.Activity.Teacher!.FirstName + " " + assignment.Activity.Teacher.LastName,
                    assignment.ExtendedDueAt ?? assignment.Activity.ExtendedDueAt ?? assignment.Activity.DueAt,
                    assignment.Activity.PublishedAt!.Value,
                    null)
            })
            .ToListAsync(cancellationToken);
        var assignmentIds = assignedActivities.Select(item => item.Id).ToArray();
        var submissions = await _context.StudentSubmissions
            .Where(submission => assignmentIds.Contains(submission.ActivityStudentId) && submission.SchoolId == schoolId)
            .Select(submission => new
            {
                submission.ActivityStudentId,
                Response = new FamilySubmissionResponse(submission.Id, submission.Value, submission.TeacherObservation, submission.SubmittedAt,
                    submission.ReceivedAt,
                    submission.Revisions.OrderByDescending(revision => revision.Version)
                        .Select(revision => new FamilySubmissionRevisionResponse(revision.Id, revision.Version,
                            revision.FileName, revision.FileSize, revision.SubmittedAt, revision.IsCurrent,
                            revision.WasAuthorizedCorrection, revision.StorageKey != null))
                        .ToList())
            })
            .ToListAsync(cancellationToken);
        var submissionByAssignment = submissions.ToDictionary(item => item.ActivityStudentId, item => item.Response);
        var activities = assignedActivities.Select(item => item.Activity with
        {
            Submission = submissionByAssignment.GetValueOrDefault(item.Id)
        }).ToList();
        return Ok(activities);
    }

    [HttpGet("students/{studentId:int}/activities/{activityId:int}/submissions/{submissionId:int}/revisions/{revisionId:int}/file")]
    public async Task<IActionResult> DownloadSubmissionFile(int studentId, int activityId, int submissionId,
        int revisionId, CancellationToken cancellationToken)
    {
        if (!TryGetFamilyIdentity(out var familyUserId, out var schoolId)) return Forbid();
        if (!await IsLinkedStudentAsync(familyUserId, schoolId, studentId, cancellationToken)) return NotFound();
        var revision = await _context.SubmissionRevisions
            .Where(item => item.Id == revisionId && item.SchoolId == schoolId && item.StudentSubmissionId == submissionId &&
                item.StudentSubmission!.SchoolId == schoolId && item.StudentSubmission.ActivityStudent!.SchoolId == schoolId &&
                item.StudentSubmission.ActivityStudent.StudentId == studentId &&
                item.StudentSubmission.ActivityStudent.ActivityId == activityId &&
                item.StudentSubmission.ActivityStudent.Activity!.SchoolId == schoolId &&
                item.StudentSubmission.ActivityStudent.Activity.IsActive &&
                item.StudentSubmission.ActivityStudent.Activity.PublishedAt.HasValue)
            .Select(item => new { item.StorageKey, item.FileName })
            .FirstOrDefaultAsync(cancellationToken);
        if (revision is null || string.IsNullOrWhiteSpace(revision.StorageKey)) return NotFound();

        FileStream? stream;
        try { stream = _fileStorage.OpenRead(activityId, revision.StorageKey); }
        catch (InvalidDataException) { return NotFound(); }
        catch (IOException) { return StatusCode(StatusCodes.Status500InternalServerError); }
        catch (UnauthorizedAccessException) { return StatusCode(StatusCodes.Status500InternalServerError); }
        if (stream is null) return NotFound();
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(stream, "application/octet-stream", revision.FileName);
    }

    [HttpGet("students/{studentId:int}/activities/{activityId:int}/materials")]
    public async Task<IActionResult> GetActivityMaterials(int studentId, int activityId, CancellationToken cancellationToken)
    {
        if (!TryGetFamilyIdentity(out var familyUserId, out var schoolId)) return Forbid();
        if (!await CanAccessActivityAsync(familyUserId, schoolId, studentId, activityId, cancellationToken)) return NotFound();

        var materials = await _context.ActivityMaterials
            .Where(item => item.ActivityId == activityId && item.SchoolId == schoolId)
            .OrderBy(item => item.CreatedAt)
            .ThenBy(item => item.Id)
            .Select(item => new FamilyActivityMaterialResponse(
                item.Id, item.Type, item.Name, item.Description, item.Url, item.FileName, item.FileSize, item.ContentType, item.CreatedAt))
            .ToListAsync(cancellationToken);
        return Ok(materials);
    }

    [HttpGet("students/{studentId:int}/activities/{activityId:int}/materials/{materialId:int}/file")]
    public async Task<IActionResult> DownloadActivityMaterialFile(
        int studentId,
        int activityId,
        int materialId,
        CancellationToken cancellationToken)
    {
        if (!TryGetFamilyIdentity(out var familyUserId, out var schoolId)) return Forbid();
        if (!await CanAccessActivityAsync(familyUserId, schoolId, studentId, activityId, cancellationToken)) return NotFound();

        var material = await _context.ActivityMaterials.FirstOrDefaultAsync(item =>
            item.Id == materialId && item.ActivityId == activityId && item.SchoolId == schoolId && item.Type == ActivityMaterialType.File,
            cancellationToken);
        if (material is null) return NotFound();

        FileStream? stream;
        try
        {
            stream = _fileStorage.OpenRead(activityId, material.StorageKey);
        }
        catch (InvalidDataException)
        {
            return NotFound();
        }
        catch (IOException)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        if (stream is null) return NotFound();
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(stream, "application/octet-stream", material.FileName ?? material.Name);
    }

    private bool TryGetFamilyIdentity(out string familyUserId, out int schoolId)
    {
        familyUserId = User.FindFirst("sub")?.Value ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        schoolId = 0;
        return !string.IsNullOrWhiteSpace(familyUserId) &&
            int.TryParse(User.FindFirst("school_id")?.Value, out schoolId) && schoolId > 0 &&
            _currentSchool.SchoolId == schoolId;
    }

    private Task<bool> IsLinkedStudentAsync(string familyUserId, int schoolId, int studentId, CancellationToken cancellationToken) =>
        _context.FamilyStudentLinks.AnyAsync(link => link.FamilyUserId == familyUserId &&
            link.StudentId == studentId && link.SchoolId == schoolId && link.Student!.SchoolId == schoolId,
            cancellationToken);

    private async Task<bool> CanAccessActivityAsync(
        string familyUserId,
        int schoolId,
        int studentId,
        int activityId,
        CancellationToken cancellationToken)
    {
        if (!await IsLinkedStudentAsync(familyUserId, schoolId, studentId, cancellationToken)) return false;
        return await _context.ActivityStudents.AnyAsync(assignment =>
            assignment.StudentId == studentId && assignment.ActivityId == activityId && assignment.SchoolId == schoolId &&
            assignment.Activity!.SchoolId == schoolId && assignment.Activity.IsActive && assignment.Activity.PublishedAt.HasValue,
            cancellationToken);
    }
}

public sealed record FamilyStudentResponse(int Id, string FirstName, string LastName, string CourseName);
public sealed record FamilyAttendanceResponse(int Id, DateTime Date, AttendanceStatus Status);
public sealed record FamilyActivityResponse(
    int Id,
    string Title,
    string Description,
    string CourseName,
    string SubjectName,
    string TeacherName,
    DateTime DueAt,
    DateTime PublishedAt,
    FamilySubmissionResponse? Submission = null);
public sealed record FamilyActivityMaterialResponse(
    int Id,
    ActivityMaterialType Type,
    string Name,
    string? Description,
    string? Url,
    string? FileName,
    long? FileSize,
    string? ContentType,
    DateTime CreatedAt);
public sealed record FamilyScheduleResponse(DayOfWeek DayOfWeek, TimeSpan StartTime, TimeSpan EndTime, string SubjectName, string TeacherName);
public sealed record FamilyGradeResponse(int Id, string EvaluationTitle, string Value, DateTime EvaluatedAt,
    string SubjectName, string CourseName, string TeacherName, DateTime PublishedAt);
public sealed record FamilyStudentRecordResponse(int Id, StudentRecordCategory Category, string Title, string Description, DateTime RecordedAt);
public sealed record FamilySubmissionResponse(int Id, string? Value, string? TeacherFeedback, DateTime SubmittedAt, DateTime? ReviewedAt,
    IReadOnlyList<FamilySubmissionRevisionResponse> Revisions);
public sealed record FamilySubmissionRevisionResponse(int Id, int Version, string FileName, long FileSize,
    DateTime SubmittedAt, bool IsCurrent, bool WasAuthorizedCorrection, bool HasFile);

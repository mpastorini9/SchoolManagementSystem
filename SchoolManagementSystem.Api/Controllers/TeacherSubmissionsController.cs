using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.DTOs;
using SchoolManagementSystem.Api.Services;

namespace SchoolManagementSystem.Api.Controllers;

[ApiController]
[Route("api/teacher")]
[Authorize(Roles = SchoolRoles.Teacher + "," + SchoolRoles.SchoolAdministrator)]
public sealed class TeacherSubmissionsController : ControllerBase
{
    private readonly SchoolContext _context;
    private readonly ActivityFileStorage _storage;

    public TeacherSubmissionsController(SchoolContext context, ActivityFileStorage storage)
    {
        _context = context;
        _storage = storage;
    }

    [HttpGet("activities/{activityId:int}/submissions")]
    public IActionResult GetActivitySubmissions(int activityId)
    {
        var activity = _context.Activities.FirstOrDefault(item => item.Id == activityId);
        if (activity is null || !CanReview(activity.TeacherId)) return NotFound();
        var submissions = _context.StudentSubmissions
            .Where(item => item.ActivityStudent!.ActivityId == activityId)
            .Include(item => item.ActivityStudent!).ThenInclude(item => item.Student)
            .Include(item => item.ActivityStudent!).ThenInclude(item => item.Activity)
            .Include(item => item.Revisions)
            .AsEnumerable()
            .Select(item => new StudentSubmissionResponse
            {
                Id = item.Id,
                ActivityId = activityId,
                StudentId = item.ActivityStudent!.StudentId,
                StudentName = $"{item.ActivityStudent.Student!.FirstName} {item.ActivityStudent.Student.LastName}",
                SubmittedAt = item.SubmittedAt,
                ReceivedAt = item.ReceivedAt,
                Value = item.Value,
                TeacherObservation = item.TeacherObservation,
                SubmissionStatus = "Submitted",
                CurrentRevision = item.Revisions.OrderByDescending(revision => revision.Version)
                    .Select(revision => new SubmissionRevisionResponse
                    {
                        Id = revision.Id, Version = revision.Version, FileName = revision.FileName,
                        Value = revision.Value, ContentType = revision.ContentType, FileSize = revision.FileSize,
                        SubmittedAt = revision.SubmittedAt, IsCurrent = revision.IsCurrent,
                        WasAuthorizedCorrection = revision.WasAuthorizedCorrection, CreatedAt = revision.CreatedAt
                    }).FirstOrDefault(revision => revision.IsCurrent),
                Revisions = item.Revisions.OrderBy(revision => revision.Version).Select(revision => new SubmissionRevisionResponse
                {
                    Id = revision.Id, Version = revision.Version, FileName = revision.FileName,
                    Value = revision.Value, ContentType = revision.ContentType, FileSize = revision.FileSize,
                    SubmittedAt = revision.SubmittedAt, IsCurrent = revision.IsCurrent,
                    WasAuthorizedCorrection = revision.WasAuthorizedCorrection, CreatedAt = revision.CreatedAt
                }).ToList()
            }).ToList();
        return Ok(submissions);
    }

    [HttpGet("submissions/{submissionId:int}/revisions")]
    public IActionResult GetRevisions(int submissionId)
    {
        var submission = GetReviewableSubmission(submissionId);
        if (submission is null) return NotFound();
        return Ok(submission.Revisions.OrderBy(item => item.Version).Select(item => new SubmissionRevisionResponse
        {
            Id = item.Id, Version = item.Version, FileName = item.FileName, Value = item.Value,
            ContentType = item.ContentType, FileSize = item.FileSize, SubmittedAt = item.SubmittedAt,
            IsCurrent = item.IsCurrent, WasAuthorizedCorrection = item.WasAuthorizedCorrection, CreatedAt = item.CreatedAt
        }));
    }

    [HttpPut("submissions/{submissionId:int}/feedback")]
    public async Task<IActionResult> SaveFeedback(int submissionId, SaveSubmissionFeedbackRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Observation) || request.Observation.Trim().Length > 4000)
            return BadRequest("La devolución debe contener entre 1 y 4000 caracteres.");
        var submission = GetReviewableSubmission(submissionId);
        if (submission is null) return NotFound();
        submission.TeacherObservation = request.Observation.Trim();
        submission.ReceivedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("submissions/{submissionId:int}/revisions/{revisionId:int}/file")]
    public IActionResult DownloadRevision(int submissionId, int revisionId)
    {
        var submission = GetReviewableSubmission(submissionId);
        var revision = submission?.Revisions.FirstOrDefault(item => item.Id == revisionId);
        if (submission is null || revision is null || string.IsNullOrWhiteSpace(revision.StorageKey)) return NotFound();
        FileStream? stream;
        try { stream = _storage.OpenRead(submission.ActivityStudent!.ActivityId, revision.StorageKey); }
        catch (InvalidDataException) { return NotFound(); }
        catch (IOException) { return StatusCode(StatusCodes.Status500InternalServerError); }
        catch (UnauthorizedAccessException) { return StatusCode(StatusCodes.Status500InternalServerError); }
        if (stream is null) return NotFound();
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(stream, "application/octet-stream", revision.FileName);
    }

    private SchoolManagementSystem.Api.Models.StudentSubmission? GetReviewableSubmission(int id)
    {
        var submission = _context.StudentSubmissions.Where(item => item.Id == id)
            .Include(item => item.ActivityStudent!).ThenInclude(item => item.Activity)
            .Include(item => item.Revisions)
            .FirstOrDefault();
        return submission is not null && CanReview(submission.ActivityStudent!.Activity!.TeacherId) ? submission : null;
    }

    private bool CanReview(int teacherId) => User.IsInRole(SchoolRoles.SchoolAdministrator) ||
        (User.IsInRole(SchoolRoles.Teacher) && int.TryParse(User.FindFirst("teacher_id")?.Value, out var currentTeacherId) && currentTeacherId == teacherId &&
         _context.Teachers.Any(teacher => teacher.Id == currentTeacherId && teacher.IsActive));
}

public sealed record SaveSubmissionFeedbackRequest(string Observation);

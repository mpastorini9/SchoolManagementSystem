using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.DTOs;
using SchoolManagementSystem.Api.Models;
using SchoolManagementSystem.Api.Services;

namespace SchoolManagementSystem.Api.Controllers
{
    [Authorize(Roles = SchoolRoles.Student)]
    [ApiController]
    [Route("api/student")]
    public class StudentSubmissionController : ControllerBase
    {
        private const long MaxSubmissionFileSize = 25 * 1024 * 1024;
        private const int MultipartOverhead = 64 * 1024;

        private readonly SchoolContext _context;
        private readonly ActivityFileStorage _fileStorage;

        public StudentSubmissionController(SchoolContext context, ActivityFileStorage fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        [HttpGet("submissions")]
        public IActionResult GetSubmissions()
        {
            if (!int.TryParse(User.FindFirst("student_id")?.Value, out var studentId))
                return Forbid();
            if (!_context.Students.Any(student => student.Id == studentId))
                return NotFound("No se encontró el alumno.");

            var submissions = _context.StudentSubmissions
                .Where(submission => submission.ActivityStudent!.StudentId == studentId)
                .Include(submission => submission.ActivityStudent!)
                    .ThenInclude(assignment => assignment.Activity)
                .Include(submission => submission.ActivityStudent!)
                    .ThenInclude(assignment => assignment.Student)
                .Include(submission => submission.Revisions)
                .AsEnumerable()
                .Where(submission => submission.ActivityStudent?.Activity?.PublishedAt.HasValue == true)
                .OrderByDescending(submission => submission.SubmittedAt)
                .Select(ToResponse)
                .ToList();

            return Ok(submissions);
        }

        [HttpPost("activities/{activityId:int}/submissions")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(MaxSubmissionFileSize + MultipartOverhead)]
        [RequestFormLimits(MultipartBodyLengthLimit = MaxSubmissionFileSize + MultipartOverhead)]
        public async Task<IActionResult> Submit(
            int activityId,
            [FromForm] CreateStudentSubmissionRequest request,
            CancellationToken cancellationToken)
        {
            if (!int.TryParse(User.FindFirst("student_id")?.Value, out var studentId))
                return Forbid();
            var assignment = _context.ActivityStudents
                .Include(item => item.Activity)
                .Include(item => item.Student)
                .FirstOrDefault(item => item.StudentId == studentId && item.ActivityId == activityId);

            if (assignment == null)
                return NotFound("No se encontró una asignación de esta actividad para el alumno.");
            if (assignment.Activity?.PublishedAt == null)
                return Conflict("La actividad todavía no está publicada.");
            if (!assignment.Activity.IsActive)
                return Conflict("No se pueden entregar actividades inactivas.");

            var value = string.IsNullOrWhiteSpace(request.Value) ? null : request.Value.Trim();
            var hasFile = request.File is { Length: > 0 };
            if (value == null && !hasFile)
                return BadRequest("La entrega debe incluir una respuesta o un archivo.");
            if (request.File is { Length: 0 })
                return BadRequest("El archivo seleccionado está vacío.");
            if (request.File is { Length: > MaxSubmissionFileSize })
                return BadRequest("El archivo supera el tamaño máximo de 25 MB.");
            if (hasFile && !IsSafeFileName(request.File!.FileName))
                return BadRequest("El nombre del archivo no es válido.");

            var submission = _context.StudentSubmissions
                .Include(item => item.Revisions)
                .FirstOrDefault(item => item.ActivityStudentId == assignment.Id);
            var now = DateTime.UtcNow;
            var isNewSubmission = submission == null;
            submission ??= new StudentSubmission
            {
                ActivityStudentId = assignment.Id,
                ActivityStudent = assignment
            };

            var previousRevision = submission.Revisions
                .OrderByDescending(revision => revision.Version)
                .FirstOrDefault();
            var revision = new SubmissionRevision
            {
                StudentSubmission = submission,
                Version = (previousRevision?.Version ?? 0) + 1,
                Value = value,
                SubmittedAt = now,
                CreatedAt = now,
                IsCurrent = true,
                WasAuthorizedCorrection = false
            };

            foreach (var existingRevision in submission.Revisions)
                existingRevision.IsCurrent = false;

            string? storageKey = null;
            if (hasFile)
            {
                try
                {
                    storageKey = await _fileStorage.SaveAsync(activityId, request.File!, cancellationToken);
                }
                catch (InvalidDataException)
                {
                    return Conflict("No se pudo guardar el archivo en el almacenamiento configurado.");
                }
                catch (IOException)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "No se pudo guardar el archivo. Intentá nuevamente.");
                }
                catch (UnauthorizedAccessException)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                        "No hay permisos para guardar archivos en el almacenamiento configurado.");
                }

                revision.FileName = Path.GetFileName(request.File!.FileName);
                revision.ContentType = string.IsNullOrWhiteSpace(request.File.ContentType)
                    ? "application/octet-stream"
                    : request.File.ContentType;
                revision.FileSize = request.File.Length;
                revision.StorageKey = storageKey;
            }

            submission.Value = value;
            submission.SubmittedAt = now;
            submission.Revisions.Add(revision);
            if (isNewSubmission)
                _context.StudentSubmissions.Add(submission);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                if (storageKey != null)
                {
                    try
                    {
                        _fileStorage.Delete(activityId, storageKey);
                    }
                    catch
                    {
                        // Preserve the persistence failure; storage keys are confined to this activity.
                    }
                }

                return StatusCode(StatusCodes.Status500InternalServerError,
                    "La entrega se recibió, pero no se pudo guardar su registro.");
            }

            var response = ToResponse(submission);
            if (isNewSubmission)
            {
                return CreatedAtAction(
                    nameof(GetRevisions),
                    new { submissionId = submission.Id },
                    response);
            }

            return Ok(response);
        }

        [HttpGet("submissions/{submissionId:int}/revisions")]
        public IActionResult GetRevisions(int submissionId)
        {
            if (!int.TryParse(User.FindFirst("student_id")?.Value, out var studentId))
                return Forbid();
            var submission = FindStudentSubmission(studentId, submissionId);
            if (submission == null)
                return NotFound("No se encontró la entrega del alumno.");

            return Ok(submission.Revisions
                .OrderBy(revision => revision.Version)
                .Select(ToRevisionResponse)
                .ToList());
        }

        [HttpGet("submissions/{submissionId:int}/revisions/{revisionId:int}/file")]
        public IActionResult DownloadRevisionFile(int submissionId, int revisionId)
        {
            if (!int.TryParse(User.FindFirst("student_id")?.Value, out var studentId))
                return Forbid();
            var submission = FindStudentSubmission(studentId, submissionId);
            if (submission == null)
                return NotFound("No se encontró la entrega del alumno.");

            var revision = submission.Revisions.FirstOrDefault(item => item.Id == revisionId);
            if (revision == null || string.IsNullOrWhiteSpace(revision.StorageKey))
                return NotFound("La revisión no contiene un archivo adjunto.");

            FileStream? stream;
            try
            {
                stream = _fileStorage.OpenRead(submission.ActivityStudent!.ActivityId, revision.StorageKey);
            }
            catch (InvalidDataException)
            {
                return NotFound("El archivo asociado ya no está disponible.");
            }
            catch (IOException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "No se pudo leer el archivo solicitado.");
            }
            catch (UnauthorizedAccessException)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    "No hay permisos para leer el archivo solicitado.");
            }

            if (stream == null)
                return NotFound("El archivo asociado ya no está disponible.");

            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(stream, "application/octet-stream", revision.FileName);
        }

        private StudentSubmission? FindStudentSubmission(int studentId, int submissionId)
        {
            return _context.StudentSubmissions
                .Where(item => item.Id == submissionId && item.ActivityStudent!.StudentId == studentId)
                .Include(item => item.ActivityStudent!)
                    .ThenInclude(assignment => assignment.Activity)
                .Include(item => item.ActivityStudent!)
                    .ThenInclude(assignment => assignment.Student)
                .Include(item => item.Revisions)
                .AsEnumerable()
                .FirstOrDefault(item => item.ActivityStudent?.Activity?.PublishedAt.HasValue == true);
        }

        private static StudentSubmissionResponse ToResponse(StudentSubmission submission)
        {
            var revisions = submission.Revisions
                .OrderBy(item => item.Version)
                .Select(ToRevisionResponse)
                .ToList();
            return new StudentSubmissionResponse
            {
                Id = submission.Id,
                ActivityId = submission.ActivityStudent?.ActivityId ?? 0,
                StudentId = submission.ActivityStudent?.StudentId ?? 0,
                StudentName = submission.ActivityStudent?.Student == null
                    ? string.Empty
                    : $"{submission.ActivityStudent.Student.FirstName} {submission.ActivityStudent.Student.LastName}",
                SubmittedAt = submission.SubmittedAt,
                ReceivedAt = submission.ReceivedAt,
                Value = submission.Value,
                TeacherObservation = submission.TeacherObservation,
                SubmissionStatus = "Submitted",
                CurrentRevision = revisions.FirstOrDefault(item => item.IsCurrent),
                Revisions = revisions
            };
        }

        private static SubmissionRevisionResponse ToRevisionResponse(SubmissionRevision revision)
        {
            return new SubmissionRevisionResponse
            {
                Id = revision.Id,
                Version = revision.Version,
                FileName = revision.FileName,
                Value = revision.Value,
                ContentType = revision.ContentType,
                FileSize = revision.FileSize,
                SubmittedAt = revision.SubmittedAt,
                IsCurrent = revision.IsCurrent,
                WasAuthorizedCorrection = revision.WasAuthorizedCorrection,
                CreatedAt = revision.CreatedAt
            };
        }

        private static bool IsSafeFileName(string fileName)
        {
            return !string.IsNullOrWhiteSpace(fileName) &&
                fileName.Length <= 255 &&
                !Path.IsPathRooted(fileName) &&
                string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal) &&
                !fileName.Contains('/') &&
                !fileName.Contains('\\') &&
                !fileName.Any(char.IsControl) &&
                fileName.IndexOfAny(Path.GetInvalidFileNameChars()) < 0 &&
                fileName.Trim() != "." &&
                fileName.Trim() != "..";
        }
    }
}

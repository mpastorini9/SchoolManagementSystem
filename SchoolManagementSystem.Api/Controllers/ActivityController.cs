using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.DTOs;
using SchoolManagementSystem.Api.Models;

namespace SchoolManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController : ControllerBase
    {
        private readonly SchoolContext _context;

        public ActivityController(SchoolContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetActivities(
            [FromQuery] int? teacherId,
            [FromQuery] int? courseId,
            [FromQuery] int? subjectId,
            [FromQuery] bool includeInactive = false)
        {
            var query = _context.Activities
                .Include(item => item.Teacher)
                .Include(item => item.Course)
                .Include(item => item.Subject)
                .Include(item => item.TargetStudent)
                .AsQueryable();

            if (!includeInactive)
                query = query.Where(item => item.IsActive);

            if (teacherId.HasValue)
                query = query.Where(item => item.TeacherId == teacherId.Value);

            if (courseId.HasValue)
                query = query.Where(item => item.CourseId == courseId.Value);

            if (subjectId.HasValue)
                query = query.Where(item => item.SubjectId == subjectId.Value);

            var activities = query
                .OrderByDescending(item => item.CreatedAt)
                .ThenByDescending(item => item.Id)
                .ToList()
                .Select(ToResponse)
                .ToList();

            return Ok(activities);
        }

        [HttpPost]
        public IActionResult CreateActivity([FromBody] CreateActivityRequest request)
        {
            var validation = ValidateActivityRequest(
                request.TeacherId,
                request.CourseId,
                request.SubjectId,
                request.Title,
                request.Description,
                request.DueAt,
                request.TargetType,
                request.TargetStudentId);

            if (validation.Error != null)
                return validation.Error;

            var activity = new Activity
            {
                TeacherId = request.TeacherId,
                Teacher = validation.Teacher,
                CourseId = request.CourseId,
                Course = validation.Course,
                SubjectId = request.SubjectId,
                Subject = validation.Subject,
                Title = request.Title.Trim(),
                Description = request.Description.Trim(),
                DueAt = request.DueAt,
                TargetType = request.TargetType,
                TargetStudentId = request.TargetStudentId,
                TargetStudent = validation.TargetStudent,
                CreatedAt = DateTime.UtcNow,
                PublishedAt = null,
                IsActive = true
            };

            _context.Activities.Add(activity);
            _context.SaveChanges();

            return CreatedAtAction(
                nameof(GetActivity),
                new { id = activity.Id },
                ToResponse(activity));
        }

        [HttpGet("{id}")]
        public IActionResult GetActivity(int id)
        {
            var activity = QueryWithReferences()
                .FirstOrDefault(item => item.Id == id);

            if (activity == null)
                return NotFound("No se encontró la actividad.");

            return Ok(ToResponse(activity));
        }

        [HttpPut("{id}")]
        public IActionResult UpdateActivity(
            int id,
            [FromBody] UpdateActivityRequest request)
        {
            var activity = _context.Activities
                .FirstOrDefault(item => item.Id == id);

            if (activity == null)
                return NotFound("No se encontró la actividad.");

            if (!activity.IsActive || activity.PublishedAt.HasValue)
                return Conflict("Solo se pueden editar borradores activos.");

            var validation = ValidateActivityRequest(
                request.TeacherId,
                request.CourseId,
                request.SubjectId,
                request.Title,
                request.Description,
                request.DueAt,
                request.TargetType,
                request.TargetStudentId);

            if (validation.Error != null)
                return validation.Error;

            activity.TeacherId = request.TeacherId;
            activity.CourseId = request.CourseId;
            activity.SubjectId = request.SubjectId;
            activity.Teacher = validation.Teacher;
            activity.Course = validation.Course;
            activity.Subject = validation.Subject;
            activity.Title = request.Title.Trim();
            activity.Description = request.Description.Trim();
            activity.DueAt = request.DueAt;
            activity.TargetType = request.TargetType;
            activity.TargetStudentId = request.TargetStudentId;
            activity.TargetStudent = validation.TargetStudent;

            _context.SaveChanges();

            return Ok(ToResponse(activity));
        }

        [HttpPost("{id}/publish")]
        public IActionResult PublishActivity(int id)
        {
            var activity = _context.Activities
                .FirstOrDefault(item => item.Id == id);

            if (activity == null)
                return NotFound("No se encontró la actividad.");

            if (!activity.IsActive)
                return Conflict("No se puede publicar una actividad inactiva.");

            if (activity.PublishedAt.HasValue)
                return Conflict("La actividad ya está publicada.");

            var validation = ValidateActivityRequest(
                activity.TeacherId,
                activity.CourseId,
                activity.SubjectId,
                activity.Title,
                activity.Description,
                activity.DueAt,
                activity.TargetType,
                activity.TargetStudentId);

            if (validation.Error != null)
                return BadRequest($"El borrador no es válido para publicar: {GetErrorMessage(validation.Error)}");

            var recipientIds = activity.TargetType == ActivityTargetType.Course
                ? _context.Students
                    .Where(student => student.CourseId == activity.CourseId)
                    .Select(student => student.Id)
                    .ToList()
                : new List<int> { activity.TargetStudentId!.Value };

            var existingRecipientIds = _context.ActivityStudents
                .Where(item => item.ActivityId == activity.Id)
                .Select(item => item.StudentId)
                .ToHashSet();

            var newRecipients = recipientIds
                .Where(studentId => !existingRecipientIds.Contains(studentId))
                .Select(studentId => new ActivityStudent
                {
                    ActivityId = activity.Id,
                    StudentId = studentId,
                    AssignedAt = DateTime.UtcNow
                })
                .ToList();

            _context.ActivityStudents.AddRange(newRecipients);
            activity.PublishedAt = DateTime.UtcNow;
            _context.SaveChanges();

            return Ok(ToResponseWithReferences(activity));
        }

        private IQueryable<Activity> QueryWithReferences()
        {
            return _context.Activities
                .Include(item => item.Teacher)
                .Include(item => item.Course)
                .Include(item => item.Subject)
                .Include(item => item.TargetStudent);
        }

        private ActivityResponse ToResponseWithReferences(Activity activity)
        {
            _context.Entry(activity).Reference(item => item.Teacher).Load();
            _context.Entry(activity).Reference(item => item.Course).Load();
            _context.Entry(activity).Reference(item => item.Subject).Load();
            _context.Entry(activity).Reference(item => item.TargetStudent).Load();
            return ToResponse(activity);
        }

        private ActivityValidation ValidateActivityRequest(
            int teacherId,
            int courseId,
            int subjectId,
            string title,
            string description,
            DateTime dueAt,
            ActivityTargetType targetType,
            int? targetStudentId)
        {
            if (teacherId <= 0)
                return ActivityValidation.BadRequest("TeacherId debe ser mayor que cero.");
            if (courseId <= 0)
                return ActivityValidation.BadRequest("CourseId debe ser mayor que cero.");
            if (subjectId <= 0)
                return ActivityValidation.BadRequest("SubjectId debe ser mayor que cero.");
            if (string.IsNullOrWhiteSpace(title))
                return ActivityValidation.BadRequest("El título es obligatorio.");
            if (string.IsNullOrWhiteSpace(description))
                return ActivityValidation.BadRequest("La descripción es obligatoria.");
            if (dueAt == default)
                return ActivityValidation.BadRequest("La fecha de entrega es obligatoria.");
            if (!Enum.IsDefined(typeof(ActivityTargetType), targetType))
                return ActivityValidation.BadRequest("El tipo de destinatario no es válido.");

            var teacher = _context.Teachers
                .FirstOrDefault(item => item.Id == teacherId && item.IsActive);
            if (teacher == null)
                return ActivityValidation.NotFound("No se encontró un docente activo.");

            var course = _context.Courses.FirstOrDefault(item => item.Id == courseId);
            if (course == null)
                return ActivityValidation.NotFound("No se encontró el curso.");

            var subject = _context.Subjects.FirstOrDefault(item => item.Id == subjectId);
            if (subject == null)
                return ActivityValidation.NotFound("No se encontró la materia.");

            var assignedSchedule = _context.TeacherSchedules.Any(item =>
                item.TeacherId == teacherId &&
                item.CourseId == courseId &&
                item.SubjectId == subjectId);
            if (!assignedSchedule)
            {
                return ActivityValidation.BadRequest(
                    "El docente no tiene asignada esa materia para el curso seleccionado.");
            }

            Student? targetStudent = null;
            if (targetType == ActivityTargetType.Student)
            {
                if (!targetStudentId.HasValue || targetStudentId.Value <= 0)
                    return ActivityValidation.BadRequest("Debe seleccionar un alumno destinatario.");

                targetStudent = _context.Students
                    .FirstOrDefault(item => item.Id == targetStudentId.Value);
                if (targetStudent == null)
                    return ActivityValidation.NotFound("No se encontró el alumno destinatario.");
                if (targetStudent.CourseId != courseId)
                {
                    return ActivityValidation.BadRequest(
                        "El alumno destinatario no pertenece al curso seleccionado.");
                }
            }
            else if (targetStudentId.HasValue)
            {
                return ActivityValidation.BadRequest(
                    "No se debe indicar un alumno destinatario para una actividad de curso.");
            }

            return new ActivityValidation(teacher, course, subject, targetStudent, null);
        }

        private static string GetErrorMessage(IActionResult error)
        {
            if (error is ObjectResult objectResult && objectResult.Value is string message)
                return message;
            return "Verificá los datos del borrador.";
        }

        private static ActivityResponse ToResponse(Activity activity)
        {
            return new ActivityResponse
            {
                Id = activity.Id,
                TeacherId = activity.TeacherId,
                TeacherName = activity.Teacher == null
                    ? string.Empty
                    : $"{activity.Teacher.FirstName} {activity.Teacher.LastName}",
                CourseId = activity.CourseId,
                CourseName = activity.Course?.Name ?? string.Empty,
                SubjectId = activity.SubjectId,
                SubjectName = activity.Subject?.Name ?? string.Empty,
                Title = activity.Title,
                Description = activity.Description,
                DueAt = activity.DueAt,
                ExtendedDueAt = activity.ExtendedDueAt,
                TargetType = (int)activity.TargetType,
                TargetStudentId = activity.TargetStudentId,
                TargetStudentName = activity.TargetStudent == null
                    ? null
                    : $"{activity.TargetStudent.FirstName} {activity.TargetStudent.LastName}",
                CreatedAt = activity.CreatedAt,
                PublishedAt = activity.PublishedAt,
                IsActive = activity.IsActive
            };
        }

        private sealed record ActivityValidation(
            Teacher? Teacher,
            Course? Course,
            Subject? Subject,
            Student? TargetStudent,
            IActionResult? Error)
        {
            public static ActivityValidation BadRequest(string message) =>
                new(null, null, null, null, new BadRequestObjectResult(message));

            public static ActivityValidation NotFound(string message) =>
                new(null, null, null, null, new NotFoundObjectResult(message));
        }
    }
}

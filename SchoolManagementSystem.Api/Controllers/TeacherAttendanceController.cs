using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.DTOs;
using SchoolManagementSystem.Api.Models;

namespace SchoolManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherAttendanceController : ControllerBase
    {
        private readonly SchoolContext _context;

        public TeacherAttendanceController(SchoolContext context) => _context = context;

        [HttpGet("schedules")]
        public IActionResult GetSchedules()
        {
            var schedules = _context.TeacherSchedules
                .Include(schedule => schedule.Teacher)
                .Include(schedule => schedule.Course)
                .Include(schedule => schedule.Subject)
                .Where(schedule => schedule.Teacher != null && schedule.Teacher.IsActive)
                .OrderBy(schedule => schedule.DayOfWeek)
                .ThenBy(schedule => schedule.StartTime)
                .ToList()
                .Select(ToScheduleResponse)
                .ToList();
            return Ok(schedules);
        }

        [HttpPost("schedules")]
        public IActionResult CreateSchedule([FromBody] CreateTeacherScheduleRequest request)
        {
            if (request.TeacherId <= 0 || request.CourseId <= 0 || request.SubjectId <= 0)
                return BadRequest("TeacherId, CourseId and SubjectId are required.");
            if (request.EndTime <= request.StartTime) return BadRequest("EndTime must be after StartTime.");

            var teacher = _context.Teachers.FirstOrDefault(item => item.Id == request.TeacherId && item.IsActive);
            if (teacher == null) return NotFound("Active teacher not found.");
            if (!_context.Courses.Any(item => item.Id == request.CourseId)) return NotFound("Course not found.");
            if (!_context.Subjects.Any(item => item.Id == request.SubjectId)) return NotFound("Subject not found.");

            var duplicate = _context.TeacherSchedules.Any(item =>
                item.TeacherId == request.TeacherId && item.CourseId == request.CourseId &&
                item.SubjectId == request.SubjectId && item.DayOfWeek == request.DayOfWeek &&
                item.StartTime == request.StartTime && item.EndTime == request.EndTime);
            if (duplicate) return Conflict("This teaching schedule already exists.");

            var schedule = new TeacherSchedule
            {
                TeacherId = request.TeacherId,
                CourseId = request.CourseId,
                SubjectId = request.SubjectId,
                DayOfWeek = request.DayOfWeek,
                StartTime = request.StartTime,
                EndTime = request.EndTime
            };
            _context.TeacherSchedules.Add(schedule);
            _context.SaveChanges();
            _context.Entry(schedule).Reference(item => item.Teacher).Load();
            _context.Entry(schedule).Reference(item => item.Course).Load();
            _context.Entry(schedule).Reference(item => item.Subject).Load();
            return CreatedAtAction(nameof(GetSchedules), new { id = schedule.Id }, ToScheduleResponse(schedule));
        }

        [HttpGet]
        public IActionResult GetTeacherAttendances()
        {
            var attendances = _context.TeacherAttendances
                .Include(item => item.TeacherSchedule)!.ThenInclude(schedule => schedule!.Teacher)
                .Include(item => item.TeacherSchedule)!.ThenInclude(schedule => schedule!.Course)
                .Include(item => item.TeacherSchedule)!.ThenInclude(schedule => schedule!.Subject)
                .OrderByDescending(item => item.Date)
                .ToList()
                .Select(ToAttendanceResponse)
                .ToList();
            return Ok(attendances);
        }

        [HttpPost]
        public IActionResult TakeTeacherAttendance([FromBody] TakeTeacherAttendanceRequest request)
        {
            if (request.TeacherScheduleId <= 0) return BadRequest("TeacherScheduleId is required.");
            if (request.Date == default) return BadRequest("Date is required.");

            var schedule = _context.TeacherSchedules
                .Include(item => item.Teacher)
                .Include(item => item.Course)
                .Include(item => item.Subject)
                .FirstOrDefault(item => item.Id == request.TeacherScheduleId);
            if (schedule == null || schedule.Teacher == null || !schedule.Teacher.IsActive)
                return NotFound("Active teaching schedule not found.");
            if (schedule.DayOfWeek != request.Date.DayOfWeek)
                return BadRequest("The selected date does not match the schedule day.");
            if (_context.TeacherAttendances.Any(item => item.TeacherScheduleId == request.TeacherScheduleId && item.Date.Date == request.Date.Date))
                return Conflict("Attendance for this teaching activity and date already exists.");

            var attendance = new TeacherAttendance
            {
                TeacherScheduleId = schedule.Id,
                Date = request.Date.Date,
                Status = request.Status
            };
            _context.TeacherAttendances.Add(attendance);
            _context.SaveChanges();
            attendance.TeacherSchedule = schedule;
            return CreatedAtAction(nameof(GetTeacherAttendances), new { id = attendance.Id }, ToAttendanceResponse(attendance));
        }

        private static TeacherScheduleResponse ToScheduleResponse(TeacherSchedule schedule) => new()
        {
            Id = schedule.Id,
            TeacherId = schedule.TeacherId,
            TeacherName = $"{schedule.Teacher!.FirstName} {schedule.Teacher.LastName}",
            CourseId = schedule.CourseId,
            CourseName = schedule.Course!.Name,
            SubjectId = schedule.SubjectId,
            SubjectName = schedule.Subject!.Name,
            DayOfWeek = schedule.DayOfWeek,
            StartTime = schedule.StartTime,
            EndTime = schedule.EndTime,
            HoursTaught = (decimal)(schedule.EndTime - schedule.StartTime).TotalHours
        };

        private static TeacherAttendanceResponse ToAttendanceResponse(TeacherAttendance attendance)
        {
            var schedule = attendance.TeacherSchedule!;
            return new TeacherAttendanceResponse
            {
                Id = attendance.Id,
                Date = attendance.Date,
                DayOfWeek = attendance.Date.DayOfWeek,
                TeacherId = schedule.TeacherId,
                TeacherName = $"{schedule.Teacher!.FirstName} {schedule.Teacher.LastName}",
                CourseName = schedule.Course!.Name,
                SubjectName = schedule.Subject!.Name,
                HoursTaught = (decimal)(schedule.EndTime - schedule.StartTime).TotalHours,
                Status = attendance.Status
            };
        }
    }
}

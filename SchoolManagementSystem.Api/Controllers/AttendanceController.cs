using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.Models;
using SchoolManagementSystem.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace SchoolManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly SchoolContext _context;

        public AttendanceController(SchoolContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAttendances()
        {
            var attendances = _context.Attendances.ToList();
            return Ok(attendances);

        }

        [HttpPost("take-attendance")]
        public IActionResult TakeAttendance([FromBody] TakeAttendanceRequest request)
        {
            if (request.CourseId <= 0)
            {
                return BadRequest("CourseId must be greater than zero.");
            }

            if (request.Date == default)
            {
                return BadRequest("Date is required.");
            }

            if (request.AbsentStudentsIds is null)
            {
                return BadRequest("AbsentStudentsIds is required.");
            }

            var course = _context.Courses
                .Include(c => c.Students)
                .FirstOrDefault(c => c.Id == request.CourseId);

            if (course == null)
            {
                return NotFound("Course not found.");
            }

            var alreadyTaken = _context.Attendances
                .Any(a => a.Date == request.Date && a.Student.CourseId == request.CourseId);

            if (alreadyTaken)
            {
                return Conflict("Attendance for this course and date already exists.");
            }

            var attendances = new List<Attendance>();

            foreach (var student in course.Students)
            {
                var isAbsent = request.AbsentStudentsIds.Contains(student.Id);
                var attendance = new Attendance
                {
                    StudentId = student.Id,
                    Date = request.Date,
                    Status = isAbsent ? AttendanceStatus.Absent : AttendanceStatus.Present
                };
                attendances.Add(attendance);
            }

            _context.Attendances.AddRange(attendances);
            _context.SaveChanges();

            return Ok();
        }
    }
}

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
            var alreadyTaken = _context.Attendances
            .Any(a => a.Date == request.Date && a.Student.CourseId == request.CourseId);
            List<Attendance> existingAttendances = new();
            if (alreadyTaken)
            {
                //return BadRequest("Attendance for this course and date already exists.");
                 existingAttendances = _context.Attendances
                    .Where(a => a.Date == request.Date && a.Student.CourseId == request.CourseId)
                    .ToList();

            _context.Attendances.RemoveRange(existingAttendances);
            _context.SaveChanges();

            }

            

            var course = _context.Courses
                .Include(c => c.Students)
                .FirstOrDefault(c => c.Id == request.CourseId);

            if (course == null)
            {
                return NotFound("Course not found.");
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


        [HttpPost]
        public IActionResult CreateAttendance(Attendance attendance)
        {
            _context.Attendances.Add(attendance);
            _context.SaveChanges();

            return Ok(attendance);
        }
    }
}
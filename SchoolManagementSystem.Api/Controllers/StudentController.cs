using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using SchoolManagementSystem.Api.Services;

namespace SchoolManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : ControllerBase
    {
        private readonly SchoolContext _context;

        public StudentController(SchoolContext context)
        {
            _context = context;
        }


        [HttpGet]
        [Authorize(Roles = SchoolRoles.SchoolAdministrator + "," + SchoolRoles.Teacher)]
        public IActionResult GetStudents()
        {
            var query = _context.Students.AsQueryable();
            if (User.IsInRole(SchoolRoles.Teacher))
            {
                if (!int.TryParse(User.FindFirst("teacher_id")?.Value, out var teacherId)) return Forbid();
                if (!_context.Teachers.Any(teacher => teacher.Id == teacherId && teacher.IsActive)) return Forbid();
                var courseIds = _context.TeacherSchedules.Where(schedule => schedule.TeacherId == teacherId)
                    .Select(schedule => schedule.CourseId).Distinct().ToList();
                query = query.Where(student => courseIds.Contains(student.CourseId));
            }
            var students = query
                .Include(s => s.Course)
                .ToList();

            return Ok(students);
        }

        [HttpPost]
        [Authorize(Roles = SchoolRoles.SchoolAdministrator)]
        public IActionResult CreateStudent(Student student)
        {
            _context.Students.Add(student);
            _context.SaveChanges();

            return Ok(student);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.Models;

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
        public IActionResult GetStudents()
        {
            var students = _context.Students.ToList();
            return Ok(students);
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            _context.Students.Add(student);
            _context.SaveChanges();

            return Ok(student);
        }
    }
}
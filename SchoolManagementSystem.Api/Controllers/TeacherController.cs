using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.DTOs;
using SchoolManagementSystem.Api.Models;

namespace SchoolManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TeacherController : ControllerBase
    {
        private readonly SchoolContext _context;

        public TeacherController(SchoolContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetTeachers()
        {
            var teachers = _context.Teachers
                .OrderBy(teacher => teacher.LastName)
                .ThenBy(teacher => teacher.FirstName)
                .Select(teacher => new TeacherResponse
                {
                    Id = teacher.Id,
                    FirstName = teacher.FirstName,
                    LastName = teacher.LastName,
                    DocumentNumber = teacher.DocumentNumber
                })
                .ToList();

            return Ok(teachers);
        }

        [HttpPost]
        public IActionResult CreateTeacher([FromBody] CreateTeacherRequest request)
        {
            var firstName = request.FirstName?.Trim();
            var lastName = request.LastName?.Trim();
            var documentNumber = request.DocumentNumber?.Trim();

            if (string.IsNullOrWhiteSpace(firstName))
            {
                return BadRequest("FirstName is required.");
            }

            if (string.IsNullOrWhiteSpace(lastName))
            {
                return BadRequest("LastName is required.");
            }

            if (string.IsNullOrWhiteSpace(documentNumber))
            {
                return BadRequest("DocumentNumber is required.");
            }

            var documentNumberExists = _context.Teachers
                .AsEnumerable()
                .Any(teacher => string.Equals(
                    teacher.DocumentNumber,
                    documentNumber,
                    StringComparison.OrdinalIgnoreCase));

            if (documentNumberExists)
            {
                return Conflict("A teacher with this DocumentNumber already exists.");
            }

            var teacher = new Teacher
            {
                FirstName = firstName,
                LastName = lastName,
                DocumentNumber = documentNumber
            };

            _context.Teachers.Add(teacher);
            _context.SaveChanges();

            var response = new TeacherResponse
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                DocumentNumber = teacher.DocumentNumber
            };

            return CreatedAtAction(
                nameof(GetTeachers),
                new { id = teacher.Id },
                response);
        }
    }
}

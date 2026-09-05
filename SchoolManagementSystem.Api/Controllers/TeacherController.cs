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
                .Where(teacher => teacher.IsActive)
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

            // ===== VALIDACIÓN DEL DNI =====
            if (!documentNumber.All(char.IsDigit))
            {
                return BadRequest("DocumentNumber must contain only numbers.");
            }
            // ==============================

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
                DocumentNumber = documentNumber,
                IsActive = true
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

        // ===== EDITAR DOCENTE =====
        [HttpPut("{id}")]
        public IActionResult UpdateTeacher(
            int id,
            [FromBody] CreateTeacherRequest request)
        {
            var teacher = _context.Teachers
                .FirstOrDefault(teacher => teacher.Id == id);

            if (teacher == null)
            {
                return NotFound("Teacher not found.");
            }

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

            // ===== VALIDACIÓN DEL DNI =====
            if (!documentNumber.All(char.IsDigit))
            {
                return BadRequest("DocumentNumber must contain only numbers.");
            }
            // ==============================

            var documentNumberExists = _context.Teachers
                .AsEnumerable()
                .Any(existingTeacher =>
                    existingTeacher.Id != id &&
                    string.Equals(
                        existingTeacher.DocumentNumber,
                        documentNumber,
                        StringComparison.OrdinalIgnoreCase));

            if (documentNumberExists)
            {
                return Conflict(
                    "A teacher with this DocumentNumber already exists.");
            }

            teacher.FirstName = firstName;
            teacher.LastName = lastName;
            teacher.DocumentNumber = documentNumber;

            _context.SaveChanges();

            var response = new TeacherResponse
            {
                Id = teacher.Id,
                FirstName = teacher.FirstName,
                LastName = teacher.LastName,
                DocumentNumber = teacher.DocumentNumber
            };

            return Ok(response);
        }

        // ===== ELIMINAR DOCENTE (ELIMINACIÓN LÓGICA) =====
        [HttpDelete("{id}")]
        public IActionResult DeleteTeacher(int id)
        {
            var teacher = _context.Teachers
                .FirstOrDefault(teacher => teacher.Id == id);

            if (teacher == null || !teacher.IsActive)
            {
                return NotFound("Teacher not found.");
            }

            teacher.IsActive = false;

            _context.SaveChanges();

            return NoContent();
        }
        // =================================================
    }
}
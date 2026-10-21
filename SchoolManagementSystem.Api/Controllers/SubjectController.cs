using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.Models;
using Microsoft.AspNetCore.Authorization;
using SchoolManagementSystem.Api.Services;

namespace SchoolManagementSystem.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubjectController : ControllerBase
    {
        private readonly SchoolContext _context;

        public SubjectController(SchoolContext context) => _context = context;

        [HttpGet]
        public IActionResult GetSubjects() => Ok(_context.Subjects.OrderBy(subject => subject.Name).ToList());

        [HttpPost]
        [Authorize(Roles = SchoolRoles.SchoolAdministrator)]
        public IActionResult CreateSubject([FromBody] Subject subject)
        {
            var name = subject.Name?.Trim();
            if (string.IsNullOrWhiteSpace(name)) return BadRequest("Name is required.");
            if (_context.Subjects.Any(existing => existing.Name.ToLower() == name.ToLower()))
                return Conflict("A subject with this name already exists.");

            subject.Name = name;
            _context.Subjects.Add(subject);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetSubjects), new { id = subject.Id }, subject);
        }
    }
}

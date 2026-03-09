using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.Models;

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

        [HttpPost]
        public IActionResult CreateAttendance(Attendance attendance)
        {
            _context.Attendances.Add(attendance);
            _context.SaveChanges();

            return Ok(attendance);
        }
    }
}
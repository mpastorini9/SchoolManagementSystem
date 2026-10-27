using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.DTOs;
using SchoolManagementSystem.Api.Models;
using SchoolManagementSystem.Api.Services;

namespace SchoolManagementSystem.Api.Controllers;

[ApiController]
[Route("api/admin/accounts")]
[Authorize(Roles = SchoolRoles.SchoolAdministrator)]
public sealed class AccountProvisioningController : ControllerBase
{
    private readonly SchoolContext _context;
    private readonly UserManager<ApplicationUser> _users;

    public AccountProvisioningController(SchoolContext context, UserManager<ApplicationUser> users)
    {
        _context = context;
        _users = users;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProvisionAccountRequest request)
    {
        if (!int.TryParse(User.FindFirst("school_id")?.Value, out var schoolId) || schoolId <= 0)
            return Forbid();
        if (!Enum.TryParse<ProvisionedAccountRole>(request.Role, true, out var role))
            return BadRequest("Role must be Student, Teacher or Family.");
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and Password are required.");

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(), Email = request.Email.Trim(), EmailConfirmed = true,
            SchoolId = schoolId
        };
        var familyStudentIds = request.StudentIds?.Distinct().ToArray() ?? Array.Empty<int>();
        if (role == ProvisionedAccountRole.Student)
        {
            if (request.StudentId is null || request.TeacherId is not null || familyStudentIds.Length > 0 || !_context.Students.Any(x => x.Id == request.StudentId))
                return BadRequest("Provide an existing StudentId only.");
            if (_context.Users.Any(x => x.StudentId == request.StudentId)) return Conflict("The student already has an account.");
            user.StudentId = request.StudentId;
        }
        else if (role == ProvisionedAccountRole.Teacher)
        {
            if (request.TeacherId is null || request.StudentId is not null || familyStudentIds.Length > 0 || !_context.Teachers.Any(x => x.Id == request.TeacherId && x.IsActive))
                return BadRequest("Provide an active TeacherId only.");
            if (_context.Users.Any(x => x.TeacherId == request.TeacherId)) return Conflict("The teacher already has an account.");
            user.TeacherId = request.TeacherId;
        }
        else
        {
            if (request.StudentId is not null || request.TeacherId is not null || familyStudentIds.Length == 0 || familyStudentIds.Any(id => id <= 0))
                return BadRequest("Provide one or more StudentIds only for a Family account.");
            var matchingStudents = _context.Students.Count(student => familyStudentIds.Contains(student.Id) && student.SchoolId == schoolId);
            if (matchingStudents != familyStudentIds.Length)
                return BadRequest("Every StudentId must belong to the administrator's school.");
        }

        var result = await _users.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(result.Errors.Select(error => error.Description));
        var roleName = role switch
        {
            ProvisionedAccountRole.Student => SchoolRoles.Student,
            ProvisionedAccountRole.Teacher => SchoolRoles.Teacher,
            _ => SchoolRoles.Family
        };
        result = await _users.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            await _users.DeleteAsync(user);
            return StatusCode(StatusCodes.Status500InternalServerError, result.Errors.Select(error => error.Description));
        }
        if (role == ProvisionedAccountRole.Family)
        {
            var administratorId = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
            _context.FamilyStudentLinks.AddRange(familyStudentIds.Select(studentId => new FamilyStudentLink
            {
                FamilyUserId = user.Id,
                StudentId = studentId,
                SchoolId = schoolId,
                LinkedAt = DateTime.UtcNow,
                LinkedByUserId = administratorId
            }));
            try
            {
                await _context.SaveChangesAsync();
            }
            catch
            {
                await _users.DeleteAsync(user);
                return StatusCode(StatusCodes.Status500InternalServerError, "The family account could not be linked to its students.");
            }
        }
        return StatusCode(StatusCodes.Status201Created,
            new { user.Id, user.Email, role = roleName, user.StudentId, user.TeacherId, studentIds = familyStudentIds });
    }
}

public sealed class ProvisionAccountRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? StudentId { get; set; }
    public int? TeacherId { get; set; }
    public List<int>? StudentIds { get; set; }
}

public enum ProvisionedAccountRole { Student, Teacher, Family }

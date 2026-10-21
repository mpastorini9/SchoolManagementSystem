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
            return BadRequest("Role must be Student or Teacher.");
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email and Password are required.");

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(), Email = request.Email.Trim(), EmailConfirmed = true,
            SchoolId = schoolId
        };
        if (role == ProvisionedAccountRole.Student)
        {
            if (request.StudentId is null || request.TeacherId is not null || !_context.Students.Any(x => x.Id == request.StudentId))
                return BadRequest("Provide an existing StudentId only.");
            if (_context.Users.Any(x => x.StudentId == request.StudentId)) return Conflict("The student already has an account.");
            user.StudentId = request.StudentId;
        }
        else
        {
            if (request.TeacherId is null || request.StudentId is not null || !_context.Teachers.Any(x => x.Id == request.TeacherId && x.IsActive))
                return BadRequest("Provide an active TeacherId only.");
            if (_context.Users.Any(x => x.TeacherId == request.TeacherId)) return Conflict("The teacher already has an account.");
            user.TeacherId = request.TeacherId;
        }

        var result = await _users.CreateAsync(user, request.Password);
        if (!result.Succeeded) return BadRequest(result.Errors.Select(error => error.Description));
        var roleName = role == ProvisionedAccountRole.Student ? SchoolRoles.Student : SchoolRoles.Teacher;
        result = await _users.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            await _users.DeleteAsync(user);
            return StatusCode(StatusCodes.Status500InternalServerError, result.Errors.Select(error => error.Description));
        }
        return StatusCode(StatusCodes.Status201Created,
            new { user.Id, user.Email, role = roleName, user.StudentId, user.TeacherId });
    }
}

public sealed class ProvisionAccountRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? StudentId { get; set; }
    public int? TeacherId { get; set; }
}

public enum ProvisionedAccountRole { Student, Teacher }

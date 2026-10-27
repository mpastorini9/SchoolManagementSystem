using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using SchoolManagementSystem.Api.Data;
using SchoolManagementSystem.Api.Models;
using SchoolManagementSystem.Api.Services;

namespace SchoolManagementSystem.Api.Controllers;

[ApiController]
[Route("api/admin/family-links")]
[Authorize(Roles = SchoolRoles.SchoolAdministrator)]
public sealed class FamilyLinkManagementController : ControllerBase
{
    private readonly SchoolContext _context;
    private readonly UserManager<ApplicationUser> _users;

    public FamilyLinkManagementController(SchoolContext context, UserManager<ApplicationUser> users)
    {
        _context = context;
        _users = users;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? familyUserId, CancellationToken cancellationToken)
    {
        if (!TryGetSchoolId(out var schoolId)) return Forbid();
        var query = _context.FamilyStudentLinks
            .Where(link => link.SchoolId == schoolId)
            .Include(link => link.FamilyUser)
            .Include(link => link.Student)
                .ThenInclude(student => student!.Course)
            .AsQueryable();
        if (!string.IsNullOrWhiteSpace(familyUserId))
            query = query.Where(link => link.FamilyUserId == familyUserId);

        var links = await query.OrderBy(link => link.FamilyUser!.Email).ThenBy(link => link.StudentId)
            .Select(link => new
            {
                link.FamilyUserId,
                Email = link.FamilyUser!.Email,
                link.StudentId,
                StudentName = link.Student!.FirstName + " " + link.Student.LastName,
                CourseName = link.Student.Course!.Name,
                link.LinkedAt,
                link.LinkedByUserId
            })
            .ToListAsync(cancellationToken);
        return Ok(links);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFamilyStudentLinkRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetSchoolId(out var schoolId)) return Forbid();
        if (string.IsNullOrWhiteSpace(request.FamilyUserId) || request.StudentId <= 0)
            return BadRequest("FamilyUserId and a positive StudentId are required.");

        var familyUser = await _users.FindByIdAsync(request.FamilyUserId);
        if (familyUser is null || familyUser.SchoolId != schoolId || familyUser.StudentId.HasValue || familyUser.TeacherId.HasValue ||
            !await _users.IsInRoleAsync(familyUser, SchoolRoles.Family))
            return BadRequest("FamilyUserId must identify a family account in the administrator's school.");
        if (!await _context.Students.AnyAsync(student => student.Id == request.StudentId && student.SchoolId == schoolId, cancellationToken))
            return BadRequest("StudentId must identify a student in the administrator's school.");
        if (await _context.FamilyStudentLinks.AnyAsync(link =>
                link.FamilyUserId == familyUser.Id && link.StudentId == request.StudentId && link.SchoolId == schoolId,
                cancellationToken))
            return Conflict("This family account is already linked to the student.");

        var administratorId = User.FindFirst("sub")?.Value ?? User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
        var link = new FamilyStudentLink
        {
            FamilyUserId = familyUser.Id,
            StudentId = request.StudentId,
            SchoolId = schoolId,
            LinkedAt = DateTime.UtcNow,
            LinkedByUserId = administratorId
        };
        _context.FamilyStudentLinks.Add(link);
        await _context.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { familyUserId = familyUser.Id }, new
        {
            link.FamilyUserId,
            link.StudentId,
            link.SchoolId,
            link.LinkedAt,
            link.LinkedByUserId
        });
    }

    [HttpDelete("{familyUserId}/{studentId:int}")]
    public async Task<IActionResult> Revoke(string familyUserId, int studentId, CancellationToken cancellationToken)
    {
        if (!TryGetSchoolId(out var schoolId)) return Forbid();
        var link = await _context.FamilyStudentLinks.FirstOrDefaultAsync(item =>
            item.FamilyUserId == familyUserId && item.StudentId == studentId && item.SchoolId == schoolId,
            cancellationToken);
        if (link is null) return NotFound();
        _context.FamilyStudentLinks.Remove(link);
        await _context.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private bool TryGetSchoolId(out int schoolId) =>
        int.TryParse(User.FindFirst("school_id")?.Value, out schoolId) && schoolId > 0;
}

public sealed class CreateFamilyStudentLinkRequest
{
    public string FamilyUserId { get; set; } = string.Empty;
    public int StudentId { get; set; }
}

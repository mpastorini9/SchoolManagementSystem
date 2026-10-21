using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using SchoolManagementSystem.Api.Models;
using SchoolManagementSystem.Api.Data;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace SchoolManagementSystem.Api.Controllers;

[ApiController]
public sealed class AuthenticationController : Controller
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly SchoolContext _context;

    public AuthenticationController(UserManager<ApplicationUser> users, SchoolContext context)
    {
        _users = users;
        _context = context;
    }

    [AllowAnonymous]
    [HttpGet("/connect/authorize")]
    [HttpPost("/connect/authorize")]
    public async Task<IActionResult> AuthorizeOidc(CancellationToken cancellationToken)
    {
        var request = Microsoft.AspNetCore.OpenIddictServerAspNetCoreHelpers.GetOpenIddictServerRequest(HttpContext);
        if (request is null) return BadRequest("Invalid OpenID Connect request.");

        var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
        if (!result.Succeeded)
            return Challenge(new AuthenticationProperties { RedirectUri = Request.PathBase + Request.Path + Request.QueryString }, IdentityConstants.ApplicationScheme);

        var user = await _users.GetUserAsync(result.Principal!);
        if (user is null || !user.EmailConfirmed || await _users.IsLockedOutAsync(user))
            return Forbid(IdentityConstants.ApplicationScheme);
        if (user.SchoolId <= 0 || !_context.Schools.Any(school => school.Id == user.SchoolId))
            return Forbid(IdentityConstants.ApplicationScheme);
        if (user.StudentId.HasValue && !_context.Students.IgnoreQueryFilters()
                .Any(student => student.Id == user.StudentId && student.SchoolId == user.SchoolId))
            return Forbid(IdentityConstants.ApplicationScheme);
        if (user.TeacherId.HasValue && !_context.Teachers.IgnoreQueryFilters()
                .Any(teacher => teacher.Id == user.TeacherId && teacher.SchoolId == user.SchoolId && teacher.IsActive))
            return Forbid(IdentityConstants.ApplicationScheme);

        var identity = new ClaimsIdentity(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
            Claims.Name, Claims.Role);
        identity.SetClaim(Claims.Subject, user.Id);
        if (!string.IsNullOrWhiteSpace(user.Email)) identity.SetClaim(Claims.Email, user.Email);
        if (!string.IsNullOrWhiteSpace(user.UserName)) identity.SetClaim(Claims.Name, user.UserName);
        if (user.StudentId.HasValue) identity.SetClaim("student_id", user.StudentId.Value.ToString());
        if (user.TeacherId.HasValue) identity.SetClaim("teacher_id", user.TeacherId.Value.ToString());
        identity.SetClaim("school_id", user.SchoolId.ToString());
        foreach (var role in await _users.GetRolesAsync(user)) identity.AddClaim(Claims.Role, role);
        identity.SetScopes(request.GetScopes());
        identity.SetResources("school-management-api");
        foreach (var claim in identity.Claims)
            claim.SetDestinations(GetDestinations(claim, request.GetScopes()));

        return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [Authorize(AuthenticationSchemes = OpenIddict.Validation.AspNetCore.OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    [HttpGet("/connect/userinfo")]
    [HttpPost("/connect/userinfo")]
    public IActionResult UserInfo()
    {
        var claims = new Dictionary<string, object> { [Claims.Subject] = User.GetClaim(Claims.Subject)! };
        var scopes = User.GetScopes();
        if (scopes.Contains(Scopes.Email))
        {
            var email = User.GetClaim(Claims.Email);
            if (email is not null) claims[Claims.Email] = email;
        }
        if (scopes.Contains(Scopes.Profile))
        {
            var name = User.GetClaim(Claims.Name);
            if (name is not null) claims[Claims.Name] = name;
        }
        if (scopes.Contains("roles")) claims[Claims.Role] = User.FindAll(Claims.Role).Select(item => item.Value).ToArray();
        return Ok(claims);
    }

    [AllowAnonymous]
    [HttpGet("/connect/logout")]
    [HttpPost("/connect/logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        return SignOut(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [Authorize]
    [HttpGet("/api/auth/me")]
    public IActionResult Me() => Ok(new
    {
        subject = User.FindFirst(Claims.Subject)?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
        name = User.Identity?.Name,
        roles = User.FindAll(Claims.Role).Select(claim => claim.Value).ToArray(),
        studentId = User.FindFirst("student_id")?.Value,
        teacherId = User.FindFirst("teacher_id")?.Value,
        schoolId = User.FindFirst("school_id")?.Value
    });

    private static IEnumerable<string> GetDestinations(Claim claim, IEnumerable<string> scopes)
    {
        if (claim.Type is Claims.Subject or Claims.Role or "student_id" or "teacher_id" or "school_id")
            return new[] { Destinations.AccessToken };
        if (claim.Type == Claims.Name && scopes.Contains(Scopes.Profile))
            return new[] { Destinations.AccessToken, Destinations.IdentityToken };
        if (claim.Type == Claims.Email && scopes.Contains(Scopes.Email))
            return new[] { Destinations.AccessToken, Destinations.IdentityToken };
        return Array.Empty<string>();
    }
}

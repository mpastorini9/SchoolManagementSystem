using System.Security.Claims;

namespace SchoolManagementSystem.Api.Services;

public interface ICurrentSchoolContext
{
    int? SchoolId { get; }
}

public sealed class CurrentSchoolContext(IHttpContextAccessor httpContextAccessor) : ICurrentSchoolContext
{
    private int? _initializationSchoolId;

    public int? SchoolId =>
        int.TryParse(httpContextAccessor.HttpContext?.User.FindFirstValue("school_id"), out var schoolId) && schoolId > 0
            ? schoolId
            : _initializationSchoolId;

    internal void SetForInitialization(int schoolId)
    {
        if (schoolId <= 0) throw new ArgumentOutOfRangeException(nameof(schoolId));
        _initializationSchoolId = schoolId;
    }
}

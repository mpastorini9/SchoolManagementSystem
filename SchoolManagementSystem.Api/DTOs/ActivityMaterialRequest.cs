using SchoolManagementSystem.Api.Models;

namespace SchoolManagementSystem.Api.DTOs
{
    public class ActivityMaterialRequest
    {
        public ActivityMaterialType Type { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Url { get; set; }
    }
}
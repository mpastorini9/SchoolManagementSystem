namespace SchoolManagementSystem.Api.Models
{
    public class ActivityMaterial
    {
        public int Id { get; set; }

        public int ActivityId { get; set; }
        public Activity? Activity { get; set; }

        public ActivityMaterialType Type { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Url { get; set; }

        public string? FileName { get; set; }

        public string? ContentType { get; set; }

        public long? FileSize { get; set; }

        public string? StorageKey { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    public enum ActivityMaterialType
    {
        File = 1,
        Link = 2,
        External = 3
    }
}

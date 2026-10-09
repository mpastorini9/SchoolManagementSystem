namespace SchoolManagementSystem.Api.DTOs
{
    public class ActivityMaterialResponse
    {
        public int Id { get; set; }

        public int Type { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Url { get; set; }

        public string? FileName { get; set; }

        public long? FileSize { get; set; }

        public string? ContentType { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}

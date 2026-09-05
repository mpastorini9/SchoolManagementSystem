namespace SchoolManagementSystem.Api.DTOs
{
    public class ImportPreviewResponse
    {
        public string EntityType { get; set; } = string.Empty;
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public bool CanConfirm { get; set; }
        public List<ImportRowResult> Rows { get; set; } = new();
    }

    public class ImportRowResult
    {
        public int RowNumber { get; set; }
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Values { get; set; } = new();
    }
}

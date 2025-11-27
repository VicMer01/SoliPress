namespace DocumentApprovalSystem.Models
{
    public class FileSettings
    {
        public int MaxFileSizeInMB { get; set; } = 10;
        public string[] AllowedExtensions { get; set; } = new[] { ".pdf", ".docx", ".jpg", ".jpeg", ".png" };
    }
}

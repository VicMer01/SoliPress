namespace DocumentApprovalSystem.Models
{
    public class AuditLog
    {
        public int Id { get; set; }
        public int DocumentRequestId { get; set; }
        public DocumentRequest? DocumentRequest { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        public User? User { get; set; }
        
        public string Action { get; set; } = string.Empty; // "Created", "Approved", "Rejected", "Voted", "Updated"
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        public string? Details { get; set; } // JSON con información adicional
    }
}

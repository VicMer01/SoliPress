using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentApprovalSystem.Models
{
    public class DocumentHistory
    {
        public int Id { get; set; }

        public int DocumentRequestId { get; set; }

        [ForeignKey("DocumentRequestId")]
        public DocumentRequest? DocumentRequest { get; set; }

        public string Action { get; set; } = string.Empty;

        public DateTime Date { get; set; } = DateTime.Now;

        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public string? Notes { get; set; }
    }
}

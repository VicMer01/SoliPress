using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentApprovalSystem.Models
{
    public class DocumentRequest
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio.")]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string Status { get; set; } = "Pending";

        [Required(ErrorMessage = "El usuario solicitante es obligatorio.")]
        public string RequestedByUserId { get; set; } = string.Empty;

        [ForeignKey("RequestedByUserId")]
        public User? RequestedByUser { get; set; }

        public string? ApprovedByUserId { get; set; }

        [ForeignKey("ApprovedByUserId")]
        public User? ApprovedByUser { get; set; }

        public string DocumentPath { get; set; } = string.Empty;

        public string? Comments { get; set; }

        // Histories navigation
        public ICollection<DocumentHistory> Histories { get; set; } = new List<DocumentHistory>();
        public ICollection<ApprovalVote> Votes { get; set; } = new List<ApprovalVote>();
    }
}

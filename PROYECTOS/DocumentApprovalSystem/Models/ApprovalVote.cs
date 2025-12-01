using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentApprovalSystem.Models
{
    public enum VoteDecision
    {
        Approve,
        Reject
    }

    public class ApprovalVote
    {
        public int Id { get; set; }

        [Required]
        public int DocumentRequestId { get; set; }

        [ForeignKey("DocumentRequestId")]
        public DocumentRequest? DocumentRequest { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        public VoteDecision Decision { get; set; }

        public string? Comments { get; set; }

        public DateTime VoteDate { get; set; } = DateTime.Now;
    }
}

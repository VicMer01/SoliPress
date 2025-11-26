using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DocumentApprovalSystem.Models
{
    public class User : IdentityUser
    {
        [Required]
        public string FullName { get; set; } = string.Empty;

        public string? Department { get; set; }

        public string? Role { get; set; }

        // Navigation collections
        public ICollection<DocumentRequest> RequestedDocumentRequests { get; set; } = new List<DocumentRequest>();
        public ICollection<DocumentRequest> ApprovedDocumentRequests { get; set; } = new List<DocumentRequest>();
        public ICollection<DocumentHistory> DocumentHistories { get; set; } = new List<DocumentHistory>();
    }
}

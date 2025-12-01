using DocumentApprovalSystem.Models;

namespace DocumentApprovalSystem.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(int documentId, string userId, string action, string ipAddress, string? details = null);
        Task<List<AuditLog>> GetDocumentHistoryAsync(int documentId);
        Task<List<AuditLog>> GetUserActionsAsync(string userId);
        Task<List<AuditLog>> GetAllLogsAsync();
    }
}

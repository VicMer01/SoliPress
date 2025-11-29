using DocumentApprovalSystem.Models;

namespace DocumentApprovalSystem.Services
{
    public interface IDashboardService
    {
        Task<int> GetTotalDocumentsAsync();
        Task<int> GetPendingDocumentsAsync();
        Task<int> GetApprovedDocumentsAsync();
        Task<int> GetRejectedDocumentsAsync();
        Task<List<AuditLog>> GetRecentActivityAsync(int count = 10);
        Task<double> GetApprovalRateAsync();
        Task<Dictionary<string, int>> GetDocumentsByStatusAsync();
    }
}

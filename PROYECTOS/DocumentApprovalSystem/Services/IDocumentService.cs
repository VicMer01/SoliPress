using DocumentApprovalSystem.Models;
using Microsoft.AspNetCore.Components.Forms;

namespace DocumentApprovalSystem.Services;

public interface IDocumentService
{
    Task<List<DocumentRequest>> GetAllAsync();
    Task<DocumentRequest?> GetByIdAsync(int id);
    Task<DocumentRequest> CreateAsync(DocumentRequest request, IBrowserFile file);
    Task<bool> ApproveAsync(int id, string userId, string notes);
    Task<bool> RejectAsync(int id, string userId, string notes);
    Task<List<DocumentHistory>> GetHistoryAsync(int documentId);

    // Methods used in Razor components (aliases or overloads)
    Task<List<DocumentRequest>> GetDocumentsAsync();
    Task<DocumentRequest?> GetDocumentByIdAsync(int id);
    Task<DocumentRequest> CreateDocumentAsync(DocumentRequest request);
}

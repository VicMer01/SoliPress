using DocumentApprovalSystem.Models;

namespace DocumentApprovalSystem.Services
{
    public interface IEmailNotificationService
    {
        Task SendNewDocumentNotificationAsync(DocumentRequest request, List<User> approvers);
        Task SendApprovalStatusNotificationAsync(DocumentRequest request, User requester);
        Task SendVoteReminderAsync(DocumentRequest request, User approver);
        Task<bool> IsConfiguredAsync();
    }
}

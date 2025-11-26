using DocumentApprovalSystem.Models;

namespace DocumentApprovalSystem.Services
{
    public interface IApprovalService
    {
        Task VoteAsync(int requestId, string userId, VoteDecision decision, string? comments = null);
        Task<bool> CheckApprovalStatusAsync(int requestId);
        Task<ApprovalConfig> GetConfigAsync();
        Task UpdateConfigAsync(ApprovalConfig config);
        Task<List<ApprovalVote>> GetVotesForRequestAsync(int requestId);
        Task<bool> HasUserVotedAsync(int requestId, string userId);
    }
}

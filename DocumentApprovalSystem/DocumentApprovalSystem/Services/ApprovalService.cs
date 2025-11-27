using DocumentApprovalSystem.Data;
using DocumentApprovalSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DocumentApprovalSystem.Services
{
    public class ApprovalService : IApprovalService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailNotificationService _emailService;
        private readonly ILogger<ApprovalService> _logger;

        public ApprovalService(
            ApplicationDbContext context, 
            UserManager<User> userManager,
            IEmailNotificationService emailService,
            ILogger<ApprovalService> logger)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ApprovalConfig> GetConfigAsync()
        {
            var config = await _context.ApprovalConfigs.FirstOrDefaultAsync();
            if (config == null)
            {
                config = new ApprovalConfig();
                _context.ApprovalConfigs.Add(config);
                await _context.SaveChangesAsync();
            }
            return config;
        }

        public async Task UpdateConfigAsync(ApprovalConfig config)
        {
            _context.ApprovalConfigs.Update(config);
            await _context.SaveChangesAsync();
        }

        public async Task VoteAsync(int requestId, string userId, VoteDecision decision, string? comments = null)
        {
            var existingVote = await _context.ApprovalVotes
                .FirstOrDefaultAsync(v => v.DocumentRequestId == requestId && v.UserId == userId);

            if (existingVote != null)
            {
                existingVote.Decision = decision;
                existingVote.Comments = comments;
                existingVote.VoteDate = DateTime.Now;
            }
            else
            {
                var vote = new ApprovalVote
                {
                    DocumentRequestId = requestId,
                    UserId = userId,
                    Decision = decision,
                    Comments = comments,
                    VoteDate = DateTime.Now
                };
                _context.ApprovalVotes.Add(vote);
            }

            await _context.SaveChangesAsync();
            await CheckApprovalStatusAsync(requestId);
        }

        public async Task<List<ApprovalVote>> GetVotesForRequestAsync(int requestId)
        {
            return await _context.ApprovalVotes
                .Include(v => v.User)
                .Where(v => v.DocumentRequestId == requestId)
                .ToListAsync();
        }

        public async Task<bool> HasUserVotedAsync(int requestId, string userId)
        {
            return await _context.ApprovalVotes.AnyAsync(v => v.DocumentRequestId == requestId && v.UserId == userId);
        }

        public async Task<bool> CheckApprovalStatusAsync(int requestId)
        {
            var request = await _context.DocumentRequests
                .Include(r => r.Votes)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null) return false;

            var config = await GetConfigAsync();
            var votes = request.Votes.ToList();
            var approvers = await _userManager.GetUsersInRoleAsync("Aprobador");
            int totalApprovers = approvers.Count;
            
            // If no approvers, cannot approve
            if (totalApprovers == 0) return false;

            int approveVotes = votes.Count(v => v.Decision == VoteDecision.Approve);
            int rejectVotes = votes.Count(v => v.Decision == VoteDecision.Reject);
            int totalVotes = votes.Count;

            bool isApproved = false;
            bool isRejected = false;

            switch (config.Mode)
            {
                case ApprovalMode.Majority:
                    // Simple majority of total possible approvers? Or majority of cast votes?
                    // Usually majority of total approvers to ensure quorum, or majority of votes if quorum met.
                    // Let's go with: if approve > totalApprovers / 2
                    if (approveVotes > totalApprovers / 2.0) isApproved = true;
                    else if (rejectVotes >= totalApprovers / 2.0) isRejected = true; // Tie or more rejects
                    break;

                case ApprovalMode.Unanimous:
                    if (approveVotes == totalApprovers) isApproved = true;
                    else if (rejectVotes > 0) isRejected = true;
                    break;

                case ApprovalMode.MinVotes:
                    if (approveVotes >= config.ThresholdValue) isApproved = true;
                    // For rejection in min votes, it's harder to say unless we know max possible votes remaining
                    // But if it's impossible to reach threshold, then reject?
                    // For simplicity, let's just mark approved if threshold met.
                    // If all voted and not met, then reject.
                    else if (totalVotes == totalApprovers && approveVotes < config.ThresholdValue) isRejected = true;
                    break;

                case ApprovalMode.MinPercentage:
                    double percentage = (double)approveVotes / totalApprovers * 100;
                    if (percentage >= config.ThresholdValue) isApproved = true;
                    else if (totalVotes == totalApprovers && percentage < config.ThresholdValue) isRejected = true;
                    break;
            }

            if (isApproved)
            {
                request.Status = "Approved";
                // We could set ApprovedByUserId to the last voter or system?
                // Or maybe keep it null or set to the last approver.
                // Let's leave it as is or update if needed.
            }
            else if (isRejected)
            {
                request.Status = "Rejected";
            }
            // If neither, stays Pending

            await _context.SaveChangesAsync();

            // Send email notification to requester if status changed
            if (isApproved || isRejected)
            {
                try
                {
                    _logger.LogInformation($"Attempting to send email notification for request {requestId}. Status: {request.Status}");
                    var requester = await _context.Users.FindAsync(request.RequestedByUserId);
                    if (requester != null)
                    {
                        _logger.LogInformation($"Sending email to {requester.Email}");
                        await _emailService.SendApprovalStatusNotificationAsync(request, requester);
                        _logger.LogInformation("Email sent successfully");
                    }
                    else
                    {
                        _logger.LogWarning($"Requester not found for request {requestId}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to send email notification for request {requestId}");
                }
            }

            return isApproved;
        }
    }
}

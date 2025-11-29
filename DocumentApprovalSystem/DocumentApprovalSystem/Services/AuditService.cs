using DocumentApprovalSystem.Data;
using DocumentApprovalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentApprovalSystem.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogActionAsync(int documentId, string userId, string action, string ipAddress, string? details = null)
        {
            try
            {
                var auditLog = new AuditLog
                {
                    DocumentRequestId = documentId,
                    UserId = userId,
                    Action = action,
                    Timestamp = DateTime.Now,
                    IpAddress = ipAddress,
                    Details = details
                };

                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Audit log created: {action} by user {userId} on document {documentId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to create audit log for document {documentId}");
                // Don't throw - auditing failure shouldn't break the main operation
            }
        }

        public async Task<List<AuditLog>> GetDocumentHistoryAsync(int documentId)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .Where(a => a.DocumentRequestId == documentId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetUserActionsAsync(string userId)
        {
            return await _context.AuditLogs
                .Include(a => a.DocumentRequest)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .Take(50) // Last 50 actions
                .ToListAsync();
        }

        public async Task<List<AuditLog>> GetAllLogsAsync()
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .Include(a => a.DocumentRequest)
                .OrderByDescending(a => a.Timestamp)
                .Take(1000) // Limit to last 1000 for performance
                .ToListAsync();
        }
    }
}

using DocumentApprovalSystem.Data;
using DocumentApprovalSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentApprovalSystem.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(ApplicationDbContext context, ILogger<DashboardService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> GetTotalDocumentsAsync()
        {
            return await _context.DocumentRequests.CountAsync();
        }

        public async Task<int> GetPendingDocumentsAsync()
        {
            return await _context.DocumentRequests
                .CountAsync(d => d.Status == "Pending");
        }

        public async Task<int> GetApprovedDocumentsAsync()
        {
            return await _context.DocumentRequests
                .CountAsync(d => d.Status == "Approved");
        }

        public async Task<int> GetRejectedDocumentsAsync()
        {
            return await _context.DocumentRequests
                .CountAsync(d => d.Status == "Rejected");
        }

        public async Task<List<AuditLog>> GetRecentActivityAsync(int count = 10)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .Include(a => a.DocumentRequest)
                .OrderByDescending(a => a.Timestamp)
                .Take(count)
                .ToListAsync();
        }

        public async Task<double> GetApprovalRateAsync()
        {
            var total = await GetTotalDocumentsAsync();
            if (total == 0) return 0;

            var approved = await GetApprovedDocumentsAsync();
            return Math.Round((double)approved / total * 100, 1);
        }

        public async Task<Dictionary<string, int>> GetDocumentsByStatusAsync()
        {
            var documents = await _context.DocumentRequests
                .GroupBy(d => d.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            return documents.ToDictionary(d => d.Status, d => d.Count);
        }
    }
}

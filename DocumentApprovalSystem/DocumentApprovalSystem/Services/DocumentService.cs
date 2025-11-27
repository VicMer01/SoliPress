using DocumentApprovalSystem.Data;
using DocumentApprovalSystem.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace DocumentApprovalSystem.Services;

public class DocumentService : IDocumentService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private const long MaxFileSize = 100 * 1024 * 1024; // 100 MB
    private readonly ILogger<DocumentService> _logger;
    private readonly IEmailNotificationService _emailService;
    private readonly UserManager<User> _userManager;
    private readonly IFileValidationService _fileValidationService;

    public DocumentService(
        ApplicationDbContext context, 
        IWebHostEnvironment environment, 
        ILogger<DocumentService> logger,
        IEmailNotificationService emailService,
        UserManager<User> userManager,
        IFileValidationService fileValidationService)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
        _emailService = emailService;
        _userManager = userManager;
        _fileValidationService = fileValidationService;
    }

    public async Task<List<DocumentRequest>> GetAllAsync()
    {
        return await _context.DocumentRequests
            .Include(d => d.RequestedByUser)
            .Include(d => d.ApprovedByUser)
            .OrderByDescending(d => d.CreatedDate)
            .ToListAsync();
    }

    public async Task<DocumentRequest?> GetByIdAsync(int id)
    {
        return await _context.DocumentRequests
            .Include(d => d.RequestedByUser)
            .Include(d => d.ApprovedByUser)
            .Include(d => d.Histories)
                .ThenInclude(h => h.User)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<DocumentRequest> CreateAsync(DocumentRequest request, IBrowserFile file)
    {
        try 
        {
            _logger.LogInformation($"Starting file upload. Name: {file.Name}, Size: {file.Size}");

            _logger.LogInformation($"Starting file upload. Name: {file.Name}, Size: {file.Size}");

            var validationResult = await _fileValidationService.ValidateFileAsync(file);
            if (!validationResult.IsValid)
            {
                throw new Exception(validationResult.ErrorMessage);
            }

            var extension = Path.GetExtension(file.Name).ToLowerInvariant();

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
            {
                _logger.LogInformation($"Creating uploads directory: {uploadsFolder}");
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);
            _logger.LogInformation($"Saving file to: {filePath}");

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                // Use a slightly larger limit for OpenReadStream to be safe, or match MaxFileSize
                await file.OpenReadStream(MaxFileSize).CopyToAsync(stream);
            }

            if (new FileInfo(filePath).Length == 0)
            {
                _logger.LogWarning("File created but is empty.");
                throw new Exception("File upload failed: File is empty.");
            }

            _logger.LogInformation("File saved successfully.");

            request.DocumentPath = $"uploads/{fileName}";
            request.CreatedDate = DateTime.Now;
            request.Status = "Pending";

            _context.DocumentRequests.Add(request);
            await _context.SaveChangesAsync();

            var history = new DocumentHistory
            {
                DocumentRequestId = request.Id,
                Action = "Created",
                Date = DateTime.Now,
                UserId = request.RequestedByUserId,
                Notes = "Document request created."
            };
            _context.DocumentHistories.Add(history);
            await _context.SaveChangesAsync();

            // Send email notification to approvers
            try
            {
                var approvers = await _userManager.GetUsersInRoleAsync("Aprobador");
                if (approvers.Any())
                {
                    await _emailService.SendNewDocumentNotificationAsync(request, approvers.ToList());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email notifications");
                // Don't fail the request creation if email fails
            }

            return request;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document request.");
            throw;
        }
    }

    public async Task<bool> ApproveAsync(int id, string userId, string notes)
    {
        return await UpdateStatusAsync(id, userId, "Approved", notes);
    }

    public async Task<bool> RejectAsync(int id, string userId, string notes)
    {
        return await UpdateStatusAsync(id, userId, "Rejected", notes);
    }

    private async Task<bool> UpdateStatusAsync(int id, string userId, string status, string notes)
    {
        var document = await _context.DocumentRequests.FindAsync(id);
        if (document == null) return false;

        document.Status = status;
        document.ApprovedByUserId = userId;

        var history = new DocumentHistory
        {
            DocumentRequestId = document.Id,
            Action = status,
            Date = DateTime.Now,
            UserId = userId,
            Notes = notes
        };

        _context.DocumentHistories.Add(history);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<DocumentHistory>> GetHistoryAsync(int documentId)
    {
        return await _context.DocumentHistories
            .Include(h => h.User)
            .Where(h => h.DocumentRequestId == documentId)
            .OrderByDescending(h => h.Date)
            .ToListAsync();
    }

    // Implementation of alias methods for Razor components
    public async Task<List<DocumentRequest>> GetDocumentsAsync()
    {
        return await GetAllAsync();
    }

    public async Task<DocumentRequest?> GetDocumentByIdAsync(int id)
    {
        return await GetByIdAsync(id);
    }

    public async Task<DocumentRequest> CreateDocumentAsync(DocumentRequest request)
    {
        // For now, just save without file if not provided via the other method
        // In a real scenario, we might want to handle file upload separately or ensure it's passed
        request.CreatedDate = DateTime.Now;
        request.Status = "Pending";

        _context.DocumentRequests.Add(request);
        await _context.SaveChangesAsync();

        var history = new DocumentHistory
        {
            DocumentRequestId = request.Id,
            Action = "Created",
            Date = DateTime.Now,
            UserId = request.RequestedByUserId,
            Notes = "Document request created."
        };
        _context.DocumentHistories.Add(history);
        await _context.SaveChangesAsync();

        // Send email notification to approvers
        try
        {
            var approvers = await _userManager.GetUsersInRoleAsync("Aprobador");
            if (approvers.Any())
            {
                await _emailService.SendNewDocumentNotificationAsync(request, approvers.ToList());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notifications");
        }

        return request;
    }
}

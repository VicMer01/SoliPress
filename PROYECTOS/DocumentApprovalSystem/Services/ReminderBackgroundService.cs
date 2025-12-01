using DocumentApprovalSystem.Data;
using DocumentApprovalSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DocumentApprovalSystem.Services
{
    public class ReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ReminderBackgroundService> _logger;
        private readonly IConfiguration _configuration;

        public ReminderBackgroundService(
            IServiceProvider serviceProvider,
            ILogger<ReminderBackgroundService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Reminder Background Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckPendingDocumentsAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking for pending documents.");
                }

                // Get interval from config or default to 24 hours
                var intervalHours = _configuration.GetValue<int>("ReminderSettings:IntervalHours", 24);
                await Task.Delay(TimeSpan.FromHours(intervalHours), stoppingToken);
            }

            _logger.LogInformation("Reminder Background Service is stopping.");
        }

        private async Task CheckPendingDocumentsAsync()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                // Get threshold from config or default to 2 days
                var thresholdDays = _configuration.GetValue<int>("ReminderSettings:PendingDaysThreshold", 2);
                var cutoffDate = DateTime.Now.AddDays(-thresholdDays);

                var pendingDocuments = await context.DocumentRequests
                    .Include(d => d.RequestedByUser)
                    .Where(d => d.Status == "Pending" && d.CreatedDate <= cutoffDate)
                    .ToListAsync();

                if (pendingDocuments.Any())
                {
                    _logger.LogInformation($"Found {pendingDocuments.Count} pending documents older than {thresholdDays} days.");

                    var approvers = await userManager.GetUsersInRoleAsync("Aprobador");
                    if (!approvers.Any())
                    {
                        _logger.LogWarning("No approvers found to send reminders to.");
                        return;
                    }

                    foreach (var doc in pendingDocuments)
                    {
                        // Send notifications
                        foreach (var approver in approvers)
                        {
                            // In-App Notification
                            await notificationService.CreateNotificationAsync(
                                approver.Id,
                                "Recordatorio: Documento Pendiente",
                                $"El documento '{doc.Title}' lleva más de {thresholdDays} días esperando aprobación.",
                                $"documents/details/{doc.Id}",
                                "Warning"
                            );
                        }

                        // Email Notification (Batch or individual? Let's do batch per doc for now to reuse service)
                        // Note: Ideally we would batch all docs in one email per approver, but for simplicity reusing existing logic
                        // Or we can just log that we would send an email.
                        // Let's try to send an email using a generic notification method if available, or skip if too spammy.
                        // Given the previous requirement to disable emails, we should respect that flag inside the service.
                        // We'll call the service, it handles the flag.
                        
                        // However, SendNewDocumentNotificationAsync is for NEW docs. 
                        // We might need a generic SendReminderAsync. 
                        // For now, let's stick to In-App notifications as primary for reminders to avoid spamming email templates not designed for this.
                        // Or we can use the existing one with a slight context mismatch, but better to be clean.
                        // Let's just log for email part or implement a simple reminder email later.
                        // For this iteration, In-App is sufficient and safer.
                    }
                }
            }
        }
    }
}

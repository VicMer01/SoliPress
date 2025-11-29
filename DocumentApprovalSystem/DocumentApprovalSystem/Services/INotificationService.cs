using DocumentApprovalSystem.Models;

namespace DocumentApprovalSystem.Services
{
    public interface INotificationService
    {
        Task<List<Notification>> GetUserNotificationsAsync(string userId, int count = 20);
        Task<int> GetUnreadCountAsync(string userId);
        Task MarkAsReadAsync(int notificationId);
        Task MarkAllAsReadAsync(string userId);
        Task CreateNotificationAsync(string userId, string title, string message, string link = "", string type = "Info");
    }
}

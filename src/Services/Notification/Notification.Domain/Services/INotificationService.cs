using Notification.Domain.Models;

namespace Notification.Domain.Services
{
    public interface INotificationService
    {
        Task<Models.Notification> SendEmailAsync(Guid userId, string email, string subject, string body);
        Task<Models.Notification> SendSmsAsync(Guid userId, string phoneNumber, string message);
        Task<Models.Notification> SendByTemplateAsync(Guid userId, string templateName, string recipient, Dictionary<string, string> parameters);
        Task<IEnumerable<Models.Notification>> GetUserNotificationsAsync(Guid userId);
        Task<NotificationTemplate> GetTemplateByNameAsync(string name);
        Task<NotificationTemplate> CreateTemplateAsync(string name, string subject, string body, NotificationType type);
        Task<NotificationTemplate> UpdateTemplateAsync(Guid id, string subject, string body);
    }
}
using Notification.Domain.Models;

namespace Notification.Domain.Repositories
{
    public interface INotificationTemplateRepository
    {
        Task<NotificationTemplate> GetByIdAsync(Guid id);
        Task<NotificationTemplate> GetByNameAsync(string name);
        Task<IEnumerable<NotificationTemplate>> GetAllAsync();
        Task<NotificationTemplate> CreateAsync(NotificationTemplate template);
        Task UpdateAsync(NotificationTemplate template);
        Task DeleteAsync(Guid id);
    }
}
namespace Notification.Domain.Repositories
{
    public interface INotificationRepository
    {
        Task<Models.Notification> GetByIdAsync(Guid id);
        Task<IEnumerable<Models.Notification>> GetByUserIdAsync(Guid userId);
        Task<Models.Notification> CreateAsync(Models.Notification notification);
        Task UpdateAsync(Models.Notification notification);
    }
}
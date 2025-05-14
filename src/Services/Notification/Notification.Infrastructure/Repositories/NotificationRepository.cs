// Notification.Infrastructure/Repositories/NotificationRepository.cs
using Microsoft.EntityFrameworkCore;
using Notification.Domain.Repositories;
using Notification.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notification.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly NotificationDbContext _dbContext;

        public NotificationRepository(NotificationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Domain.Models.Notification> GetByIdAsync(Guid id)
        {
            return await _dbContext.Notifications.FindAsync(id);
        }

        public async Task<IEnumerable<Domain.Models.Notification>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Domain.Models.Notification> CreateAsync(Domain.Models.Notification notification)
        {
            await _dbContext.Notifications.AddAsync(notification);
            await _dbContext.SaveChangesAsync();
            return notification;
        }

        public async Task UpdateAsync(Domain.Models.Notification notification)
        {
            _dbContext.Notifications.Update(notification);
            await _dbContext.SaveChangesAsync();
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Notification.Domain.Models;
using Notification.Domain.Repositories;
using Notification.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notification.Infrastructure.Repositories
{
    public class NotificationTemplateRepository : INotificationTemplateRepository
    {
        private readonly NotificationDbContext _dbContext;

        public NotificationTemplateRepository(NotificationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<NotificationTemplate> GetByIdAsync(Guid id)
        {
            return await _dbContext.Templates.FindAsync(id);
        }

        public async Task<NotificationTemplate> GetByNameAsync(string name)
        {
            return await _dbContext.Templates
                .FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task<IEnumerable<NotificationTemplate>> GetAllAsync()
        {
            return await _dbContext.Templates.ToListAsync();
        }

        public async Task<NotificationTemplate> CreateAsync(NotificationTemplate template)
        {
            await _dbContext.Templates.AddAsync(template);
            await _dbContext.SaveChangesAsync();
            return template;
        }

        public async Task UpdateAsync(NotificationTemplate template)
        {
            _dbContext.Templates.Update(template);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var template = await _dbContext.Templates.FindAsync(id);
            if (template != null)
            {
                _dbContext.Templates.Remove(template);
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}
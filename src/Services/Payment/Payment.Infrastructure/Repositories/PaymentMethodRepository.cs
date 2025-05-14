using Microsoft.EntityFrameworkCore;
using Payment.Domain.Models;
using Payment.Domain.Repositories;
using Payment.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payment.Infrastructure.Repositories
{
    public class PaymentMethodRepository : IPaymentMethodRepository
    {
        private readonly PaymentDbContext _dbContext;

        public PaymentMethodRepository(PaymentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaymentMethod> GetByIdAsync(Guid id)
        {
            return await _dbContext.PaymentMethods
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<PaymentMethod>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.PaymentMethods
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.IsDefault)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<PaymentMethod> CreateAsync(PaymentMethod paymentMethod)
        {
            await _dbContext.PaymentMethods.AddAsync(paymentMethod);
            await _dbContext.SaveChangesAsync();
            return paymentMethod;
        }

        public async Task UpdateAsync(PaymentMethod paymentMethod)
        {
            _dbContext.PaymentMethods.Update(paymentMethod);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var paymentMethod = await _dbContext.PaymentMethods.FindAsync(id);
            if (paymentMethod != null)
            {
                _dbContext.PaymentMethods.Remove(paymentMethod);
                await _dbContext.SaveChangesAsync();
            }
        }

        public async Task<PaymentMethod> GetDefaultForUserAsync(Guid userId)
        {
            return await _dbContext.PaymentMethods
                .FirstOrDefaultAsync(p => p.UserId == userId && p.IsDefault);
        }
    }
}
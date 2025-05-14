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
    public class TransactionRepository : ITransactionRepository
    {
        private readonly PaymentDbContext _dbContext;

        public TransactionRepository(PaymentDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Transaction> GetByIdAsync(Guid id)
        {
            return await _dbContext.Transactions
                .Include(t => t.PaymentMethod)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId)
        {
            return await _dbContext.Transactions
                .Include(t => t.PaymentMethod)
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task<Transaction> CreateAsync(Transaction transaction)
        {
            await _dbContext.Transactions.AddAsync(transaction);
            await _dbContext.SaveChangesAsync();
            return transaction;
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            _dbContext.Transactions.Update(transaction);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Transaction>> GetPendingTransactionsAsync()
        {
            return await _dbContext.Transactions
                .Include(t => t.PaymentMethod)
                .Where(t => t.Status == TransactionStatus.Pending)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
        }
    }
}
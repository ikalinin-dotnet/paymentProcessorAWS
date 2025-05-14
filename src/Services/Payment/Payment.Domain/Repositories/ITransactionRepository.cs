namespace Payment.Domain.Repositories
{
    using Payment.Domain.Models;
    
    public interface ITransactionRepository
    {
        Task<Transaction> GetByIdAsync(Guid id);
        Task<IEnumerable<Transaction>> GetByUserIdAsync(Guid userId);
        Task<Transaction> CreateAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task<IEnumerable<Transaction>> GetPendingTransactionsAsync();
    }
}
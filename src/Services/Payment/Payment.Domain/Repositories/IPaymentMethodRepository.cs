namespace Payment.Domain.Repositories
{
    using Payment.Domain.Models;
    
    public interface IPaymentMethodRepository
    {
        Task<PaymentMethod> GetByIdAsync(Guid id);
        Task<IEnumerable<PaymentMethod>> GetByUserIdAsync(Guid userId);
        Task<PaymentMethod> CreateAsync(PaymentMethod paymentMethod);
        Task UpdateAsync(PaymentMethod paymentMethod);
        Task DeleteAsync(Guid id);
        Task<PaymentMethod> GetDefaultForUserAsync(Guid userId);
    }
}
namespace Payment.Domain.Services
{
    using Payment.Domain.Models;
    
    public interface IPaymentService
    {
        Task<Transaction> InitiatePaymentAsync(Guid userId, decimal amount, string currency, Guid? paymentMethodId = null);
        Task<PaymentMethod> AddPaymentMethodAsync(Guid userId, PaymentMethodType type, string token, bool setAsDefault = false);
        Task<IEnumerable<PaymentMethod>> GetUserPaymentMethodsAsync(Guid userId);
        Task<Transaction> GetTransactionAsync(Guid transactionId);
        Task<IEnumerable<Transaction>> GetUserTransactionsAsync(Guid userId);
        Task DeletePaymentMethodAsync(Guid paymentMethodId, Guid userId);
        Task SetDefaultPaymentMethodAsync(Guid paymentMethodId, Guid userId);
    }
}
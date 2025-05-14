namespace Payment.Domain.Services
{
    using Payment.Domain.Models;
    
    public interface IPaymentProcessor
    {
        Task<(bool IsSuccessful, string? TransactionId, string? ErrorMessage)> ProcessPaymentAsync(
            decimal amount, 
            string currency, 
            PaymentMethod paymentMethod);
            
        Task<PaymentMethod> CreatePaymentMethodAsync(
            Guid userId, 
            PaymentMethodType type, 
            string token, 
            bool setAsDefault = false);
    }
}
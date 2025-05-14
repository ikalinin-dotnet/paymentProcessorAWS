namespace Payment.Domain.Services
{
    using Payment.Domain.Models;
    using Payment.Domain.Repositories;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    
    public class PaymentService : IPaymentService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IPaymentMethodRepository _paymentMethodRepository;
        private readonly IPaymentProcessor _paymentProcessor;
        private readonly IEventPublisher _eventPublisher;
        
        public PaymentService(
            ITransactionRepository transactionRepository,
            IPaymentMethodRepository paymentMethodRepository,
            IPaymentProcessor paymentProcessor,
            IEventPublisher eventPublisher)
        {
            _transactionRepository = transactionRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _paymentProcessor = paymentProcessor;
            _eventPublisher = eventPublisher;
        }
        
        public async Task<Transaction> InitiatePaymentAsync(Guid userId, decimal amount, string currency, Guid? paymentMethodId = null)
        {
            PaymentMethod paymentMethod;
            
            if (paymentMethodId.HasValue)
            {
                paymentMethod = await _paymentMethodRepository.GetByIdAsync(paymentMethodId.Value);
                if (paymentMethod == null || paymentMethod.UserId != userId)
                {
                    throw new InvalidOperationException("Payment method not found or does not belong to the user");
                }
            }
            else
            {
                // Get default payment method
                paymentMethod = await _paymentMethodRepository.GetDefaultForUserAsync(userId);
                if (paymentMethod == null)
                {
                    throw new InvalidOperationException("No default payment method found for user");
                }
            }
            
            var transaction = new Transaction(userId, amount, currency, paymentMethod);
            await _transactionRepository.CreateAsync(transaction);
            
            // Publish payment initiated event
            await _eventPublisher.PublishAsync("payment-initiated", new Events.PaymentInitiatedEvent
            {
                TransactionId = transaction.Id,
                UserId = userId,
                Amount = amount,
                Currency = currency,
                InitiatedAt = DateTime.UtcNow
            });
            
            // Process payment
            transaction.MarkAsProcessing();
            await _transactionRepository.UpdateAsync(transaction);
            
            var (isSuccessful, externalTransactionId, errorMessage) = 
                await _paymentProcessor.ProcessPaymentAsync(amount, currency, paymentMethod);
                
            if (isSuccessful)
            {
                transaction.MarkAsSuccessful(externalTransactionId!);
            }
            else
            {
                transaction.MarkAsFailed(errorMessage!);
            }
            
            await _transactionRepository.UpdateAsync(transaction);
            
            // Publish payment processed event
            await _eventPublisher.PublishAsync("payment-processed", new Events.PaymentProcessedEvent
            {
                TransactionId = transaction.Id,
                UserId = userId,
                Amount = amount,
                Currency = currency,
                IsSuccessful = isSuccessful,
                ErrorMessage = errorMessage,
                ProcessedAt = DateTime.UtcNow
            });
            
            return transaction;
        }
        
        public async Task<PaymentMethod> AddPaymentMethodAsync(Guid userId, PaymentMethodType type, string token, bool setAsDefault = false)
        {
            var paymentMethod = await _paymentProcessor.CreatePaymentMethodAsync(userId, type, token, setAsDefault);
            
            await _paymentMethodRepository.CreateAsync(paymentMethod);
            
            if (setAsDefault)
            {
                // Unset any existing default payment methods
                var existingMethods = await _paymentMethodRepository.GetByUserIdAsync(userId);
                foreach (var method in existingMethods)
                {
                    if (method.Id != paymentMethod.Id && method.IsDefault)
                    {
                        method.UnsetDefault();
                        await _paymentMethodRepository.UpdateAsync(method);
                    }
                }
            }
            
            return paymentMethod;
        }
        
        public async Task<IEnumerable<PaymentMethod>> GetUserPaymentMethodsAsync(Guid userId)
        {
            return await _paymentMethodRepository.GetByUserIdAsync(userId);
        }
        
        public async Task<Transaction> GetTransactionAsync(Guid transactionId)
        {
            return await _transactionRepository.GetByIdAsync(transactionId);
        }
        
        public async Task<IEnumerable<Transaction>> GetUserTransactionsAsync(Guid userId)
        {
            return await _transactionRepository.GetByUserIdAsync(userId);
        }
        
        public async Task DeletePaymentMethodAsync(Guid paymentMethodId, Guid userId)
        {
            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(paymentMethodId);
            if (paymentMethod == null || paymentMethod.UserId != userId)
            {
                throw new InvalidOperationException("Payment method not found or does not belong to the user");
            }
            
            await _paymentMethodRepository.DeleteAsync(paymentMethodId);
        }
        
        public async Task SetDefaultPaymentMethodAsync(Guid paymentMethodId, Guid userId)
        {
            var paymentMethod = await _paymentMethodRepository.GetByIdAsync(paymentMethodId);
            if (paymentMethod == null || paymentMethod.UserId != userId)
            {
                throw new InvalidOperationException("Payment method not found or does not belong to the user");
            }
            
            // Unset any existing default payment methods
            var existingMethods = await _paymentMethodRepository.GetByUserIdAsync(userId);
            foreach (var method in existingMethods)
            {
                if (method.IsDefault)
                {
                    method.UnsetDefault();
                    await _paymentMethodRepository.UpdateAsync(method);
                }
            }
            
            // Set new default
            paymentMethod.SetAsDefault();
            await _paymentMethodRepository.UpdateAsync(paymentMethod);
        }
    }
}
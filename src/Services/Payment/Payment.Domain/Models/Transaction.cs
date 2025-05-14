namespace Payment.Domain.Models
{
    public class Transaction
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }
        public TransactionStatus Status { get; private set; }
        public PaymentMethod PaymentMethod { get; private set; }
        public string? ExternalTransactionId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }
        public string? ErrorMessage { get; private set; }

        private Transaction() { } // For EF Core

        public Transaction(Guid userId, decimal amount, string currency, PaymentMethod paymentMethod)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Amount = amount;
            Currency = currency;
            Status = TransactionStatus.Pending;
            PaymentMethod = paymentMethod;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsSuccessful(string externalTransactionId)
        {
            Status = TransactionStatus.Successful;
            ExternalTransactionId = externalTransactionId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string errorMessage)
        {
            Status = TransactionStatus.Failed;
            ErrorMessage = errorMessage;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsProcessing()
        {
            Status = TransactionStatus.Processing;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
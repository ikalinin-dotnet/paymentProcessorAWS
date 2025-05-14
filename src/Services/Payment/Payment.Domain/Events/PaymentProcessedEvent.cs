namespace Payment.Domain.Events
{
    public class PaymentProcessedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
}
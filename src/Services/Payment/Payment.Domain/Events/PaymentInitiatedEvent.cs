namespace Payment.Domain.Events
{
    public class PaymentInitiatedEvent
    {
        public Guid TransactionId { get; set; }
        public Guid UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime InitiatedAt { get; set; }
    }
}
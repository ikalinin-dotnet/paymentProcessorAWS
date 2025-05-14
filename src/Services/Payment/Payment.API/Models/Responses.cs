namespace Payment.API.Models
{
    public class ProcessPaymentResponse
    {
        public Guid TransactionId { get; set; }
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
    
    public class PaymentMethodResponse
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string? CardLastFour { get; set; }
        public string? CardBrand { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    
    public class TransactionResponse
    {
        public Guid Id { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ExternalTransactionId { get; set; }
        public PaymentMethodResponse PaymentMethod { get; set; }
    }
}
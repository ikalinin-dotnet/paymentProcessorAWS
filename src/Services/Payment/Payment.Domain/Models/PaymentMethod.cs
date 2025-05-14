namespace Payment.Domain.Models
{
    public class PaymentMethod
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public PaymentMethodType Type { get; private set; }
        public string? CardLastFour { get; private set; }
        public string? CardBrand { get; private set; }
        public string? ExternalPaymentMethodId { get; private set; }
        public bool IsDefault { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? UpdatedAt { get; private set; }

        private PaymentMethod() { } // For EF Core

        public PaymentMethod(Guid userId, PaymentMethodType type, string externalPaymentMethodId, string? cardLastFour = null, string? cardBrand = null)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Type = type;
            ExternalPaymentMethodId = externalPaymentMethodId;
            CardLastFour = cardLastFour;
            CardBrand = cardBrand;
            CreatedAt = DateTime.UtcNow;
        }

        public void SetAsDefault()
        {
            IsDefault = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UnsetDefault()
        {
            IsDefault = false;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
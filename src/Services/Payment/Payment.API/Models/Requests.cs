using Payment.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Payment.API.Models
{
    public class ProcessPaymentRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        public decimal Amount { get; set; }
        
        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency code must be exactly 3 characters.")]
        public string Currency { get; set; }
        
        public Guid? PaymentMethodId { get; set; }
    }
    
    public class AddPaymentMethodRequest
    {
        [Required]
        public PaymentMethodType Type { get; set; }
        
        [Required(ErrorMessage = "Payment token is required.")]
        public string Token { get; set; }
        
        public bool SetAsDefault { get; set; } = false;
    }
}
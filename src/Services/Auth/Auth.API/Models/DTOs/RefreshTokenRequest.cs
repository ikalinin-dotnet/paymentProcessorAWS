using System.ComponentModel.DataAnnotations;

namespace PaymentProcessor.Services.Auth.API.Models.DTOs
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
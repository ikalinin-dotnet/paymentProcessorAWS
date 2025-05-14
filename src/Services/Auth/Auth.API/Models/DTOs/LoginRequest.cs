using System.ComponentModel.DataAnnotations;

namespace PaymentProcessor.Services.Auth.API.Models.DTOs
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        [Required]
        public string Password { get; set; }
    }
}
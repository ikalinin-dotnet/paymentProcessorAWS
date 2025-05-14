namespace PaymentProcessor.Services.User.API.Models.DTOs;

public class UserProfileResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty; 
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public AddressResponse? Address { get; set; }
    public List<PaymentMethodResponse> PaymentMethods { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
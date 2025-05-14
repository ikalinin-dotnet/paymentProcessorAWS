namespace PaymentProcessor.Services.User.API.Models.DTOs;

public class UpdateUserProfileRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public AddressRequest? Address { get; set; }
}

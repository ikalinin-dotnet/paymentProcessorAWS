namespace PaymentProcessor.Services.User.API.Models.DTOs;

public class PaymentMethodResponse
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Last4 { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public bool IsDefault { get; set; }
}
namespace PaymentProcessor.Services.User.API.Models.DTOs;
public class CreatePaymentMethodRequest
{
    public string Type { get; set; } = string.Empty; // "visa", "mastercard", "amex", etc.
    public string Name { get; set; } = string.Empty; // Cardholder name
    public string Last4 { get; set; } = string.Empty; // Last 4 digits
    public DateTime? ExpiryDate { get; set; }
}
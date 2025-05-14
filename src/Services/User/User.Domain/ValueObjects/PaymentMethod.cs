namespace PaymentProcessor.Services.User.Domain.ValueObjects;

public class PaymentMethod
{
    public string Id { get; private set; }
    public string Type { get; private set; } // VISA, Mastercard, PayPal, etc.
    public string Token { get; private set; } // Payment gateway token
    public string Name { get; private set; } // Card/account name
    public string Last4 { get; private set; } // Last 4 digits of card
    public DateTime? ExpiryDate { get; private set; }
    public bool IsDefault { get; internal set; }
    public DateTime CreatedAt { get; private set; }

    // For EF Core
    protected PaymentMethod() 
    {
        Id = string.Empty;
        Type = string.Empty;
        Token = string.Empty;
        Name = string.Empty;
        Last4 = string.Empty;
    }

    public PaymentMethod(
        string type, 
        string token, 
        string name, 
        string last4, 
        DateTime? expiryDate = null, 
        bool isDefault = false)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Type cannot be empty", nameof(type));
        
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token cannot be empty", nameof(token));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (string.IsNullOrWhiteSpace(last4))
            throw new ArgumentException("Last 4 digits cannot be empty", nameof(last4));

        Id = Guid.NewGuid().ToString();
        Type = type;
        Token = token;
        Name = name;
        Last4 = last4;
        ExpiryDate = expiryDate;
        IsDefault = isDefault;
        CreatedAt = DateTime.UtcNow;
    }

    public string GetMaskedNumber()
    {
        return $"**** **** **** {Last4}";
    }

    public bool IsExpired()
    {
        if (ExpiryDate.HasValue)
        {
            return ExpiryDate.Value < DateTime.UtcNow;
        }
        
        return false;
    }
}
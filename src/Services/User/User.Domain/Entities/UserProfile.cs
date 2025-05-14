using PaymentProcessor.Services.User.Domain.ValueObjects;

namespace PaymentProcessor.Services.User.Domain.Entities;

public class UserProfile
{
    public string Id { get; private set; }
    public string UserId { get; private set; }
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public Address? Address { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public List<PaymentMethod> PaymentMethods { get; private set; } = new();

    // For EF Core
    protected UserProfile() 
    {
        Id = string.Empty;
        UserId = string.Empty;
        Email = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    public UserProfile(
        string userId, 
        string email, 
        string firstName, 
        string lastName, 
        string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        Id = Guid.NewGuid().ToString();
        UserId = userId;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdatePersonalInfo(string firstName, string lastName, string? phoneNumber)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            FirstName = firstName;
        
        if (!string.IsNullOrWhiteSpace(lastName))
            LastName = lastName;

        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateAddress(Address address)
    {
        Address = address;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddPaymentMethod(PaymentMethod paymentMethod)
    {
        if (PaymentMethods.Any(p => p.Token == paymentMethod.Token))
            throw new InvalidOperationException("Payment method already exists");

        PaymentMethods.Add(paymentMethod);
        UpdatedAt = DateTime.UtcNow;
    }

    public void RemovePaymentMethod(string paymentMethodId)
    {
        var paymentMethod = PaymentMethods.FirstOrDefault(p => p.Id == paymentMethodId);
        if (paymentMethod == null)
            throw new InvalidOperationException("Payment method not found");

        PaymentMethods.Remove(paymentMethod);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDefaultPaymentMethod(string paymentMethodId)
    {
        var paymentMethod = PaymentMethods.FirstOrDefault(p => p.Id == paymentMethodId);
        if (paymentMethod == null)
            throw new InvalidOperationException("Payment method not found");

        foreach (var pm in PaymentMethods)
        {
            pm.IsDefault = pm.Id == paymentMethodId;
        }

        UpdatedAt = DateTime.UtcNow;
    }
}
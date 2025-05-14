namespace PaymentProcessor.Services.Auth.Domain.Entities;

public class User
{
    public string Id { get; private set; }
    public string Email { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public DateTime RegisteredAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public bool EmailConfirmed { get; private set; }

    // For EF Core
    protected User() 
    {
        Id = string.Empty;
        Email = string.Empty;
        FirstName = string.Empty;
        LastName = string.Empty;
    }

    public User(string email, string firstName, string lastName, string? phoneNumber = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));
        
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty", nameof(lastName));

        Id = Guid.NewGuid().ToString();
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        RegisteredAt = DateTime.UtcNow;
        EmailConfirmed = false;
    }

    public string FullName => $"{FirstName} {LastName}";

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void UpdatePersonalInfo(string firstName, string lastName, string? phoneNumber)
    {
        if (!string.IsNullOrWhiteSpace(firstName))
            FirstName = firstName;
        
        if (!string.IsNullOrWhiteSpace(lastName))
            LastName = lastName;

        PhoneNumber = phoneNumber;
    }
}
namespace PaymentProcessor.Services.User.Domain.ValueObjects;

public class Address
{
    public string Line1 { get; private set; }
    public string? Line2 { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string PostalCode { get; private set; }
    public string Country { get; private set; }

    // For EF Core
    protected Address() 
    {
        Line1 = string.Empty;
        City = string.Empty;
        State = string.Empty;
        PostalCode = string.Empty;
        Country = string.Empty;
    }

    public Address(
        string line1, 
        string city, 
        string state, 
        string postalCode, 
        string country, 
        string? line2 = null)
    {
        if (string.IsNullOrWhiteSpace(line1))
            throw new ArgumentException("Line 1 cannot be empty", nameof(line1));
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty", nameof(city));
        
        if (string.IsNullOrWhiteSpace(state))
            throw new ArgumentException("State cannot be empty", nameof(state));
        
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new ArgumentException("Postal code cannot be empty", nameof(postalCode));
        
        if (string.IsNullOrWhiteSpace(country))
            throw new ArgumentException("Country cannot be empty", nameof(country));

        Line1 = line1;
        Line2 = line2;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }

    public string GetFormattedAddress()
    {
        var addressLines = new List<string> { Line1 };
        
        if (!string.IsNullOrWhiteSpace(Line2))
            addressLines.Add(Line2);
        
        addressLines.Add($"{City}, {State} {PostalCode}");
        addressLines.Add(Country);
        
        return string.Join(Environment.NewLine, addressLines);
    }
}
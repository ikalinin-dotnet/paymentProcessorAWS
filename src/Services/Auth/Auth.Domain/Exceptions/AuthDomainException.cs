namespace PaymentProcessor.Services.Auth.Domain.Exceptions;

public class AuthDomainException : Exception
{
    public AuthDomainException()
    { }

    public AuthDomainException(string message)
        : base(message)
    { }

    public AuthDomainException(string message, Exception innerException)
        : base(message, innerException)
    { }
}
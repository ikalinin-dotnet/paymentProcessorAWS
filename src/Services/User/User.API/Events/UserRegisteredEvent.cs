using PaymentProcessor.BuildingBlocks.EventBus.Events;

namespace PaymentProcessor.Services.User.API.Events;

public record UserRegisteredEvent : IntegrationEvent
{
    public string UserId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
}
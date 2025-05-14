using PaymentProcessor.BuildingBlocks.EventBus;

namespace PaymentProcessor.Services.Auth.API.Events
{
    public record UserRegisteredEvent : IntegrationEvent
    {
        public string UserId { get; set; }
        public string Email { get; set; }
    }
}
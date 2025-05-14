using PaymentProcessor.BuildingBlocks.EventBus;
using System;

namespace PaymentProcessor.Services.Auth.API.Events
{
    public record UserLoggedInEvent : IntegrationEvent
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public DateTime LoginTime { get; set; }
    }
}
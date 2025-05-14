using System.Threading.Tasks;

namespace PaymentProcessor.BuildingBlocks.EventBus.Interfaces
{
    public interface IIntegrationEventHandler<in TIntegrationEvent>
        where TIntegrationEvent : IntegrationEvent
    {
        Task HandleAsync(TIntegrationEvent @event);
    }
}
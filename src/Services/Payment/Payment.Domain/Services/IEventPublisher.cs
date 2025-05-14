namespace Payment.Domain.Services
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(string topic, T eventMessage);
    }
}
using Microsoft.Extensions.DependencyInjection;
using PaymentProcessor.BuildingBlocks.EventBus.Interfaces;
using PaymentProcessor.BuildingBlocks.EventBus.Kafka;

namespace PaymentProcessor.BuildingBlocks.EventBus.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEventBus(this IServiceCollection services, KafkaConnectionSettings settings)
        {
            services.AddSingleton(settings);
            services.AddSingleton<IEventBus, KafkaEventBus>();
            
            return services;
        }
    }
}
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using PaymentProcessor.BuildingBlocks.EventBus.Interfaces;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace PaymentProcessor.BuildingBlocks.EventBus.Kafka
{
    public class KafkaEventBus : IEventBus, IDisposable
    {
        private readonly KafkaConnectionSettings _settings;
        private readonly ILogger<KafkaEventBus> _logger;
        private readonly IProducer<string, string> _producer;
        private readonly IServiceProvider _serviceProvider;
        private bool _disposedValue;

        public KafkaEventBus(
            KafkaConnectionSettings settings,
            ILogger<KafkaEventBus> logger,
            IServiceProvider serviceProvider)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            var config = new ProducerConfig
            {
                BootstrapServers = _settings.BootstrapServers,
                ClientId = _settings.ClientId
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishAsync<TIntegrationEvent>(TIntegrationEvent @event)
            where TIntegrationEvent : IntegrationEvent
        {
            var eventName = @event.GetType().Name;
            var topic = GetTopicName(eventName);

            _logger.LogInformation("Publishing event {EventName} to topic {Topic}", eventName, topic);

            var message = new Message<string, string>
            {
                Key = @event.Id.ToString(),
                Value = JsonSerializer.Serialize(@event)
            };

            try
            {
                var deliveryResult = await _producer.ProduceAsync(topic, message);
                _logger.LogInformation("Event {EventName} published to topic {Topic} with status {Status}",
                    eventName, topic, deliveryResult.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error publishing event {EventName} to topic {Topic}", eventName, topic);
                throw;
            }
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            // Implemented by the consumer service as background service
            // This method is called during service registration to register handlers
            _logger.LogInformation("Subscription of {EventHandler} to {Event}",
                typeof(TH).Name, typeof(T).Name);
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            // Subscription management is handled by the consumer service
            _logger.LogInformation("Unsubscription of {EventHandler} from {Event}",
                typeof(TH).Name, typeof(T).Name);
        }

        private string GetTopicName(string eventName)
        {
            return $"{_settings.DefaultTopic}-{eventName}";
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    _producer?.Dispose();
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
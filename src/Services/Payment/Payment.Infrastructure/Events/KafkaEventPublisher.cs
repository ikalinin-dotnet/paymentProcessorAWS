using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Payment.Domain.Services;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Payment.Infrastructure.Events
{
    public class KafkaEventPublisher : IEventPublisher
    {
        private readonly IConfiguration _configuration;
        private readonly IProducer<string, string> _producer;

        public KafkaEventPublisher(IConfiguration configuration)
        {
            _configuration = configuration;

            var config = new ProducerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                ClientId = _configuration["Kafka:ClientId"]
            };

            _producer = new ProducerBuilder<string, string>(config).Build();
        }

        public async Task PublishAsync<T>(string topic, T eventMessage)
        {
            try
            {
                string serializedMessage = JsonSerializer.Serialize(eventMessage);
                string fullTopic = $"{_configuration["Kafka:TopicPrefix"]}.{topic}";

                var message = new Message<string, string>
                {
                    Key = Guid.NewGuid().ToString(), // Using a UUID as the key
                    Value = serializedMessage
                };

                var deliveryResult = await _producer.ProduceAsync(fullTopic, message);

                if (deliveryResult.Status == PersistenceStatus.NotPersisted)
                {
                    throw new Exception($"Message not persisted to Kafka: {deliveryResult.Message}");
                }
            }
            catch (ProduceException<string, string> ex)
            {
                // Log the error properly
                throw new Exception($"Error publishing event to Kafka: {ex.Message}", ex);
            }
        }
    }
}
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notification.Domain.Services;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Notification.Infrastructure.Events
{
    public class PaymentEventConsumer : BackgroundService
    {
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly ILogger<PaymentEventConsumer> _logger;
        private readonly ConsumerConfig _consumerConfig;

        public PaymentEventConsumer(
            IConfiguration configuration,
            INotificationService notificationService,
            ILogger<PaymentEventConsumer> logger)
        {
            _configuration = configuration;
            _notificationService = notificationService;
            _logger = logger;
            
            _consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _configuration["Kafka:BootstrapServers"],
                GroupId = _configuration["Kafka:GroupId"],
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false
            };
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Payment Event Consumer started");

            var paymentInitiatedTopic = $"{_configuration["Kafka:TopicPrefix"]}.payment-initiated";
            var paymentProcessedTopic = $"{_configuration["Kafka:TopicPrefix"]}.payment-processed";

            using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
            consumer.Subscribe(new[] { paymentInitiatedTopic, paymentProcessedTopic });

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    try
                    {
                        var consumeResult = consumer.Consume(stoppingToken);
                        _logger.LogInformation("Received message from topic {Topic}: {Message}", 
                            consumeResult.Topic, consumeResult.Message.Value);

                        if (consumeResult.Topic == paymentInitiatedTopic)
                        {
                            await HandlePaymentInitiatedEvent(consumeResult.Message.Value);
                        }
                        else if (consumeResult.Topic == paymentProcessedTopic)
                        {
                            await HandlePaymentProcessedEvent(consumeResult.Message.Value);
                        }

                        consumer.Commit(consumeResult);
                    }
                    catch (ConsumeException ex)
                    {
                        _logger.LogError(ex, "Error consuming message");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown
                _logger.LogInformation("Payment Event Consumer shutting down");
            }
            finally
            {
                consumer.Close();
            }
        }

        private async Task HandlePaymentInitiatedEvent(string json)
        {
            try
            {
                var paymentInitiatedEvent = JsonSerializer.Deserialize<PaymentInitiatedEvent>(json);
                if (paymentInitiatedEvent == null)
                {
                    _logger.LogWarning("Failed to deserialize PaymentInitiatedEvent");
                    return;
                }

                _logger.LogInformation("Sending payment initiation notification for transaction {TransactionId}", 
                    paymentInitiatedEvent.TransactionId);

                // Send notification about payment initiation
                var parameters = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "TransactionId", paymentInitiatedEvent.TransactionId.ToString() },
                    { "Amount", paymentInitiatedEvent.Amount.ToString() },
                    { "Currency", paymentInitiatedEvent.Currency }
                };

                // This assumes a template exists with this name
                await _notificationService.SendByTemplateAsync(
                    paymentInitiatedEvent.UserId,
                    "PaymentInitiated",
                    "user@email.com", // You'd need to look up the user's contact info
                    parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment initiated event");
            }
        }

        private async Task HandlePaymentProcessedEvent(string json)
        {
            try
            {
                var paymentProcessedEvent = JsonSerializer.Deserialize<PaymentProcessedEvent>(json);
                if (paymentProcessedEvent == null)
                {
                    _logger.LogWarning("Failed to deserialize PaymentProcessedEvent");
                    return;
                }

                _logger.LogInformation("Sending payment result notification for transaction {TransactionId}", 
                    paymentProcessedEvent.TransactionId);

                // Choose template based on success/failure
                string templateName = paymentProcessedEvent.IsSuccessful ? "PaymentSuccessful" : "PaymentFailed";

                var parameters = new System.Collections.Generic.Dictionary<string, string>
                {
                    { "TransactionId", paymentProcessedEvent.TransactionId.ToString() },
                    { "Amount", paymentProcessedEvent.Amount.ToString() },
                    { "Currency", paymentProcessedEvent.Currency }
                };

                if (!paymentProcessedEvent.IsSuccessful && paymentProcessedEvent.ErrorMessage != null)
                {
                    parameters.Add("ErrorMessage", paymentProcessedEvent.ErrorMessage);
                }

                // This assumes a template exists with this name
                await _notificationService.SendByTemplateAsync(
                    paymentProcessedEvent.UserId,
                    templateName,
                    "user@email.com", // You'd need to look up the user's contact info
                    parameters);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling payment processed event");
            }
        }

        // Event classes matching the ones from the Payment service
        private class PaymentInitiatedEvent
        {
            public Guid TransactionId { get; set; }
            public Guid UserId { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; }
            public DateTime InitiatedAt { get; set; }
        }

        private class PaymentProcessedEvent
        {
            public Guid TransactionId { get; set; }
            public Guid UserId { get; set; }
            public decimal Amount { get; set; }
            public string Currency { get; set; }
            public bool IsSuccessful { get; set; }
            public string? ErrorMessage { get; set; }
            public DateTime ProcessedAt { get; set; }
        }
    }
}
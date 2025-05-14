namespace PaymentProcessor.BuildingBlocks.EventBus.Kafka
{
    public class KafkaConnectionSettings
    {
        public string BootstrapServers { get; set; }
        public string ClientId { get; set; }
        public string ConsumerGroupId { get; set; }
        public string DefaultTopic { get; set; }
    }
}
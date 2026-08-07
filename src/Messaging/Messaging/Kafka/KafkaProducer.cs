using Confluent.Kafka;
using Messaging.Events;
using System.Text.Json;

namespace Messaging.Kafka
{
    public interface IKafkaProducer
    {
        Task PublishAsync<T>(string topic, T @event) where T : IntegrationEvent;
    }

    public class KafkaProducer(ProducerConfig config) : IKafkaProducer
    {
        private readonly IProducer<string, string> producer = new ProducerBuilder<string, string>(config).Build();

        public async Task PublishAsync<T>(string topic, T @event) where T : IntegrationEvent
        {
            var message = new Message<string, string>
            {
                Key = @event.Id.ToString(), // Ensures logs and partitions can map to a specific Event ID
                Value = JsonSerializer.Serialize(@event)
            };

            await producer.ProduceAsync(topic, message);
        }
    }
}

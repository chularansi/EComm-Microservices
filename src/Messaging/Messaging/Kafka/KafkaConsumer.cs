using Confluent.Kafka;
using Messaging.Events;
using System.Text.Json;

namespace Messaging.Kafka
{
    public interface IKafkaConsumer
    {
        Task SubscribeAndListenAsync<T>(string topic, Func<T, Task> handleEventAsync, CancellationToken cancellationToken)
            where T : IntegrationEvent;
    }

    public class KafkaConsumer(ConsumerConfig config) : IKafkaConsumer
    {
        private readonly ConsumerConfig config = config;

        public Task SubscribeAndListenAsync<T>
            (string topic, Func<T, Task> handleEventAsync, CancellationToken cancellationToken)
            where T : IntegrationEvent
        {
            // Offload the blocking loop to a background thread pool worker
            return Task.Run(async () =>
            {
                using var consumer = new ConsumerBuilder<string, string>(config).Build();
                consumer.Subscribe(topic);

                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        try
                        {
                            // Consume remains synchronous/blocking per Kafka client design,
                            // but it runs safely on a background thread pool worker now.
                            var consumeResult = consumer.Consume(cancellationToken);
                            Console.WriteLine($"Raw message pulled from Kafka: {consumeResult.Message.Value}");
                            if (consumeResult?.Message?.Value == null) continue;

                            var integrationEvent = JsonSerializer.Deserialize<T>(consumeResult.Message.Value);
                            if (integrationEvent != null)
                            {
                                // Await the asynchronous business logic execution
                                await handleEventAsync(integrationEvent);

                                // Commit offset after successful async handling. THIS LINE IS SKIPPED IF ALL RETRIES FAIL!
                                consumer.Commit(consumeResult);
                            }
                        }
                        catch (ConsumeException e)
                        {
                            Console.WriteLine($"Kafka consume error occurred: {e.Error.Reason}");
                            // Give Kafka 2 seconds to breathe and refresh metadata before trying again
                            await Task.Delay(2000, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Business logic processing error: {ex.Message}");
                            await Task.Delay(2000, cancellationToken);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Triggered gracefully when cancellationToken is canceled
                }
                finally
                {
                    consumer.Close();
                }
            }, cancellationToken);
        }
    }
}

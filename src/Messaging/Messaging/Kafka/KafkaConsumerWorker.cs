using Messaging.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Messaging.Kafka
{
    public class KafkaConsumerWorker<T>(
    IKafkaConsumer kafkaConsumer,
    IKafkaProducer kafkaProducer,
    string topicName,
    IServiceScopeFactory scopeFactory, // Inject the factory instead of a specific handler instance
    ILogger<KafkaConsumerWorker<T>> logger)
    : BackgroundService where T : IntegrationEvent
    {
        private readonly IKafkaConsumer kafkaConsumer = kafkaConsumer;
        private readonly string topicName = topicName;
        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
        private readonly ILogger<KafkaConsumerWorker<T>> logger = logger;

        private const int MaxRetryAttempts = 3;
        private const int RetryDelayMilliseconds = 2000; // 2 seconds delay between retries

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Starting generic Kafka worker for event type: {Event} on topic: {Topic}",
                typeof(T).Name, topicName);

            // Forward execution to your existing consumer client setup
            await kafkaConsumer.SubscribeAndListenAsync<T>(
                topicName,
                async (@event) =>
                {
                    // Execute processing inside a controlled retry policy loop
                    int attempt = 0;
                    while (attempt < MaxRetryAttempts)
                    {
                        try
                        {
                            attempt++;

                            // Create a temporary DI scope for this specific message execution
                            using var scope = scopeFactory.CreateScope();

                            // Safely resolve your scoped services/handlers inside this message scope
                            var scopedHandler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<T>>();

                            // You can also resolve DbContext or Repositories here if needed:
                            // var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                            // Execute business logic within the scope boundary
                            await scopedHandler.HandleAsync(@event, cancellationToken);

                            // Scope ends here: DbContext connections are disposed and returned to the pool cleanly

                            return;

                        }
                        catch (Exception ex)
                        {
                            logger.LogWarning(ex, "Attempt {Attempt} failed for Event ID: {EventId}", attempt, @event.Id);

                            if (attempt >= MaxRetryAttempts)
                            {
                                // 👇 Max retries reached: route message to Dead Letter Queue (DLQ)
                                await RouteToDeadLetterQueueAsync(@event, ex.Message);
                                return; // Return safely so Kafka commits offset and advances the queue
                            }

                            // Wait before trying again
                            await Task.Delay(RetryDelayMilliseconds, cancellationToken);
                        }
                    }
                },
                cancellationToken
            );
        }

        private async Task RouteToDeadLetterQueueAsync(T @event, string errorMessage)
        {
            var dlqTopic = $"{typeof(T).Name}-dlq";

            logger.LogCritical("MOVING TO DLQ: Event ID {EventId} failed after {MaxAttempts} attempts. Routing to topic: {DlqTopic}. Error: {Error}",
                @event.Id, MaxRetryAttempts, dlqTopic, errorMessage);

            // Creates a valid object that matches the IntegrationEvent constraint
            var dlqPayload = new DeadLetterQueueEvent
            {
                Id = @event.Id, // Reuses original event ID for traceability
                OccurredOn = @event.OccurredOn,
                OriginalEvent = @event,
                ExceptionMessage = errorMessage,
                FailedAtUtc = DateTime.UtcNow
            };

            // Publish to Kafka DLQ Topic
            await kafkaProducer.PublishAsync(dlqTopic, dlqPayload);
        }
    }
}

// A Note on how this interacts with your EF Core Interceptors/Retries
// Because your scopedHandler.HandleAsync method will trigger your CQRS pipeline,
// both retry policies will now cooperate:
// If your PostgreSQL database connection drops momentarily, your global EF Core
// NpgsqlRetryingExecutionStrategy will immediately try to replay the SQL transaction 5 times silently.
// If the database stays dead or a major business validation error occurs,
// EF Core will eventually give up and bubble the exception out to this KafkaConsumerWorker.
// This consumer loop catches that failure, waits 2 seconds, and retries the entire CQRS
// command handler from scratch (including creating a brand new DI scope and
// a new database connection) up to 3 times before pushing it to the DLQ.

// -----------------
//        public class KafkaConsumerWorker<T>(
//            IKafkaConsumer kafkaConsumer,
//            string topicName,
//            IServiceScopeFactory scopeFactory, // Inject the factory instead of a specific handler instance
//            ILogger<KafkaConsumerWorker<T>> logger)
//            : BackgroundService where T : IntegrationEvent
//    {
//        private readonly IKafkaConsumer kafkaConsumer = kafkaConsumer;
//        private readonly string topicName = topicName;
//        private readonly IServiceScopeFactory scopeFactory = scopeFactory;
//        private readonly ILogger<KafkaConsumerWorker<T>> logger = logger;

//        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
//        {
//            logger.LogInformation("Starting generic Kafka worker for event type: {Event} on topic: {Topic}",
//            typeof(T).Name, topicName);

//            // Forward execution to your existing consumer client setup
//            await kafkaConsumer.SubscribeAndListenAsync<T>(
//                topicName,
//                async (@event) =>
//                {
//                    // Create a temporary DI scope for this specific message execution
//                    using var scope = scopeFactory.CreateScope();

//                    // Safely resolve your scoped services/handlers inside this message scope
//                    var scopedHandler = scope.ServiceProvider.GetRequiredService<IIntegrationEventHandler<T>>();

//                    // You can also resolve DbContext or Repositories here if needed:
//                    // var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

//                    // Execute business logic within the scope boundary
//                    await scopedHandler.HandleAsync(@event, cancellationToken);

//                    // Scope ends here: DbContext connections are disposed and returned to the pool cleanly
//                },
//                cancellationToken
//            );
//        }
//    }
//}

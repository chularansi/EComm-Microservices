using Messaging.Events;
using Messaging.Kafka;
using Microsoft.Extensions.Logging;

namespace Ordering.Infrastructure.Messaging
{
    public class BasketCheckoutIntegrationEventHandler(
        ApplicationDbContext dbContext,
        ISender sender,
        ILogger<BasketCheckoutIntegrationEventHandler> logger)
        : IIntegrationEventHandler<BasketCheckoutIntegrationEvent>
    {
        private readonly ILogger<BasketCheckoutIntegrationEventHandler> logger = logger;

        public async Task HandleAsync(BasketCheckoutIntegrationEvent message, CancellationToken cancellationToken)
        {
            await Task.Yield();

            // Check if this message was already processed in a previous thread/run
            bool alreadyProcessed = await dbContext.InboxMessages
                .AnyAsync(m => m.Id == message.Id, cancellationToken);

            if (alreadyProcessed)
            {
                logger.LogWarning("Duplicate message detected! Event ID {EventId} has already been processed. Skipping.", message.Id);
                return; // Exit early without processing or committing a duplicate
            }

            // Get the retrying execution strategy configured in Program.cs
            var strategy = dbContext.Database.CreateExecutionStrategy();

            // Execute the entire transaction block inside the strategy
            await strategy.ExecuteAsync(async () =>
            {
                // Wrap your execution block in a transaction to protect against concurrent duplicates
                using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    // Stage the inbox record to claim ownership of this Event Id
                    var inboxEntry = new InboxMessage
                    {
                        Id = message.Id,
                        EventType = message.EventType
                    };
                    await dbContext.InboxMessages.AddAsync(inboxEntry, cancellationToken);

                    // PERFORM YOUR ACTUAL BUSINESS LOGIC HERE
                    logger.LogInformation("Processing core business logic for customer: {CustomerId}", message.CustomerId);

                    var command = message.ToCreateOrderCommand();
                    logger.LogInformation("Dispatching CreateOrderCommand to Application pipeline.");

                    // Send the command down into application logic Order.CreateOrder layer
                    var result = await sender.Send(command, cancellationToken);

                    // Commit everything to the database atomically
                    await dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    logger.LogInformation("Successfully completed and saved Event ID: {EventId}", message.Id);
                }
                catch (DbUpdateException)
                {
                    // If another replica processed this event at the exact same fraction of a second, 
                    // the database Primary Key constraint on InboxMessages.Id will trip and throw this error.
                    await transaction.RollbackAsync(cancellationToken);
                    logger.LogWarning("Concurrent message clash caught! Event ID {EventId} was saved elsewhere. Rolling back.", message.Id);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    logger.LogError(ex, "Failed to process business logic for Event ID {EventId}. Rolling back.", message.Id);
                    throw; // Re-throw so your KafkaConsumer worker knows NOT to commit the Kafka offset!
                }
            });
        }
    }

    // ------------------------------Old Implementation Below (for reference)------------------------------
    //public class BasketCheckoutIntegrationEventHandler
    //    (IKafkaConsumer kafkaConsumer,
    //    IKafkaProducer kafkaProducer,
    //    //ISender sender, No need to inject ISender directly here, we will resolve it from the scope
    //    IServiceScopeFactory scopeFactory, // 👇 Inject the scope factory instead of ISender directly
    //    ILogger<BasketCheckoutIntegrationEventHandler> logger)
    //    : BackgroundService
    //{
    //    private const int MaxRetryAttempts = 3;
    //    private const int RetryDelayMilliseconds = 2000; // 2 seconds delay between retries

    //    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    //    {
    //        // Force the background worker to yield immediately so it doesn't block .NET startup
    //        await Task.Yield();

    //        logger.LogInformation("Basket Checkout Consumer Service is starting.");

    //        // Start listening to the topic. 
    //        // We pass "BasketCheckoutEvent" as the topic name and specify BasketCheckoutEvent as the type <T>.
    //        await kafkaConsumer.SubscribeAndListenAsync<BasketCheckoutIntegrationEvent>(
    //            topic: nameof(BasketCheckoutIntegrationEvent),
    //            handleEventAsync: async (basketCheckoutIntegrationEvent) =>
    //            {
    //                logger.LogInformation("Received basket checkout event for User: {UserName}", basketCheckoutIntegrationEvent.UserName);

    //                // Execute processing inside a controlled retry policy loop
    //                int attempt = 0;
    //                while (attempt < MaxRetryAttempts)
    //                {
    //                    try
    //                    {
    //                        attempt++;
    //                        await ProcessEventWithIdempotencyAsync(basketCheckoutIntegrationEvent, cancellationToken);

    //                        // If it succeeds, exit the retry loop safely
    //                        return;
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        logger.LogWarning(ex, "Attempt {Attempt} failed for Event ID: {EventId}", attempt, basketCheckoutIntegrationEvent.Id);

    //                        if (attempt >= MaxRetryAttempts)
    //                        {
    //                            // 👇 Max retries reached: route message to Dead Letter Queue (DLQ)
    //                            await RouteToDeadLetterQueueAsync(basketCheckoutIntegrationEvent, ex.Message);
    //                            return; // Return safely so Kafka commits offset and advances the queue
    //                        }

    //                        // Wait before trying again
    //                        await Task.Delay(RetryDelayMilliseconds, cancellationToken);
    //                    }
    //                }
    //            },
    //            cancellationToken: cancellationToken
    //        );
    //    }

    //    private async Task ProcessEventWithIdempotencyAsync(BasketCheckoutIntegrationEvent basketCheckoutIntegrationEvent, CancellationToken cancellationToken)
    //    {
    //        // Create a temporary scope when a message arrives
    //        using var scope = scopeFactory.CreateScope();
    //        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
    //        // Resolve your scoped ISender safely inside this block
    //        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

    //        // Check if the event was already processed
    //        var alreadyProcessed = await dbContext.Set<InboxMessage>()
    //            .AnyAsync(m => m.Id == basketCheckoutIntegrationEvent.Id, cancellationToken);

    //        if (alreadyProcessed)
    //        {
    //            logger.LogWarning("Duplicate event detected. Skipping Event ID: {EventId}", basketCheckoutIntegrationEvent.Id);
    //            return; // Return early without throwing an error (acknowledges message to Kafka)
    //        }

    //        using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    //        try
    //        {
    //            // Add to Inbox to lock the ID
    //            var inboxMessage = new InboxMessage
    //            {
    //                Id = basketCheckoutIntegrationEvent.Id,
    //                EventType = basketCheckoutIntegrationEvent.EventType
    //            };

    //            dbContext.Set<InboxMessage>().Add(inboxMessage);
    //            await dbContext.SaveChangesAsync(cancellationToken);

    //            var command = basketCheckoutIntegrationEvent.ToCreateOrderCommand();

    //            // Dispatch the command
    //            await sender.Send(command, cancellationToken);
    //            // Commit both steps simultaneously. (Write to Inbox and process the command)
    //            await transaction.CommitAsync(cancellationToken);

    //            logger.LogInformation("Successfully processed basket checkout event.");
    //            logger.LogInformation("Integration Event handled: {IntegrationEvent}", GetType().Name);
    //        }
    //        catch (Exception ex)
    //        {
    //            await transaction.RollbackAsync(cancellationToken);
    //            logger.LogError(ex, "Failed to process basket checkout event for User: {UserName}", basketCheckoutIntegrationEvent.UserName);
    //            throw; // Throwing will prevent Kafka from committing the offset (per your consumer config)
    //        }
    //    }

    //    private async Task RouteToDeadLetterQueueAsync(BasketCheckoutIntegrationEvent basketCheckoutIntegrationEvent, string errorMessage)
    //    {
    //        var dlqTopic = $"{nameof(BasketCheckoutIntegrationEvent)}-dlq";

    //        logger.LogCritical("MOVING TO DLQ: Event ID {EventId} failed after {MaxAttempts} attempts. Routing to topic: {DlqTopic}. Error: {Error}",
    //            basketCheckoutIntegrationEvent.Id, MaxRetryAttempts, dlqTopic, errorMessage);

    //        // Creates a valid object that matches the IntegrationEvent constraint
    //        var dlqPayload = new DeadLetterQueueEvent
    //        {
    //            Id = basketCheckoutIntegrationEvent.Id, // Reuses original event ID for traceability
    //            OccurredOn = basketCheckoutIntegrationEvent.OccurredOn,
    //            OriginalEvent = basketCheckoutIntegrationEvent,
    //            ExceptionMessage = errorMessage,
    //            FailedAtUtc = DateTime.UtcNow
    //        };

    //        // Publish to Kafka DLQ Topic
    //        await kafkaProducer.PublishAsync(dlqTopic, dlqPayload);
    //    }
    //}
}


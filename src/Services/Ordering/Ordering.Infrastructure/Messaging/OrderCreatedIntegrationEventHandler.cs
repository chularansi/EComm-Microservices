using Messaging.Events;
using Messaging.Kafka;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using System.Net.Sockets;

namespace Ordering.Infrastructure.Messaging
{
    public class OrderCreatedIntegrationEventHandler(ISender sender, ILogger<OrderCreatedIntegrationEventHandler> logger)
                : IIntegrationEventHandler<OrderCreatedIntegrationEvent>
    {
        //private readonly ILogger<OrderCreatedIntegrationEventHandler> logger = logger;
        //private readonly AsyncRetryPolicy retryPolicy = Policy
        //        .Handle<DbUpdateException>()
        //        .Or<SocketException>()
        //        .WaitAndRetryAsync(
        //            retryCount: 3,
        //            sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(500 * retryAttempt), // Exponential backoff: 500ms, 1000ms, 1500ms
        //            onRetry: (exception, timeSpan, retryCount, context) =>
        //            {
        //                logger.LogWarning("Database connection transient failure caught. Retrying attempt {Count} after {Timeout}ms. Error: {Message}",
        //                    retryCount, timeSpan.TotalMilliseconds, exception.Message);
        //            });
        //private readonly IServiceScopeFactory scopeFactory;

        public async Task HandleAsync(OrderCreatedIntegrationEvent @event, CancellationToken cancellationToken)
        {
            await Task.Yield();

            // Execute your core handler logic securely wrapped inside the Polly policy block
            //await retryPolicy.ExecuteAsync(async () =>
            //{
            //    logger.LogInformation("Processing OrderCreated event with ID: {OrderId}", @event.Id);

            //    logger.LogInformation("Processing OrderCreated event. Order ID: {OrderId}, Customer ID: {CustomerId}",
            //        @event.Id, @event.CustomerId);

            //    logger.LogInformation("Shipping Address: {Line}, {Country}. Current Status: {Status}",
            //        @event.ShippingAddress.AddressLine, @event.ShippingAddress.Country, @event.Status);

            //    logger.LogInformation("Order contains {Count} items.", @event.OrderItems.Count);

            //    foreach (var item in @event.OrderItems)
            //    {
            //        logger.LogInformation("-> Product ID: {ProductId}, Qty: {Qty}, Price: {Price}",
            //            item.ProductId, item.Quantity, item.Price);
            //    }

            //    // TODO: Place your order fulfillment, notification, or stock allocation logic here

            //    // Your core application logic here...
            //    //await sender.Send(new ProcessOrderFulfillmentCommand(@event.Id));

            //    await Task.CompletedTask;
            //});

            // -------------------------
            logger.LogInformation("Processing OrderCreated event with ID: {OrderId}", @event.Id);

            logger.LogInformation("Processing OrderCreated event. Order ID: {OrderId}, Customer ID: {CustomerId}",
                @event.Id, @event.CustomerId);

            logger.LogInformation("Shipping Address: {Line}, {Country}. Current Status: {Status}",
                @event.ShippingAddress.AddressLine, @event.ShippingAddress.Country, @event.Status);

            logger.LogInformation("Order contains {Count} items.", @event.OrderItems.Count);

            foreach (var item in @event.OrderItems)
            {
                logger.LogInformation("-> Product ID: {ProductId}, Qty: {Qty}, Price: {Price}",
                    item.ProductId, item.Quantity, item.Price);
            }

            await Task.CompletedTask;
        }
    }

    //public class OrderCreatedIntegrationEventHandler(
    //    IKafkaConsumer kafkaConsumer,
    //    IKafkaProducer kafkaProducer,
    //    IServiceScopeFactory scopeFactory,
    //    ILogger<OrderCreatedIntegrationEventHandler> logger)
    //    : BackgroundService
    //{
    //    private const int MaxRetryAttempts = 3;
    //    private const int RetryDelayMilliseconds = 2000; // 2 seconds delay between retries

    //    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    //    {
    //        await Task.Yield();

    //        logger.LogInformation("Order Created Consumer Service is starting.");

    //        await kafkaConsumer.SubscribeAndListenAsync<OrderCreatedIntegrationEvent>(
    //            topic: nameof(OrderCreatedIntegrationEvent),
    //            handleEventAsync: async (orderCreatedIntegrationEvent) =>
    //            {
    //                logger.LogInformation("Received order created event for User: {CustomerId}", orderCreatedIntegrationEvent.CustomerId);

    //                // Execute processing inside a controlled retry policy loop
    //                int attempt = 0;
    //                while (attempt < MaxRetryAttempts)
    //                {
    //                    try
    //                    {
    //                        attempt++;
    //                        await ProcessEventWithIdempotencyAsync(orderCreatedIntegrationEvent, cancellationToken);

    //                        return;
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        logger.LogWarning(ex, "Attempt {Attempt} failed for Event ID: {EventId}", attempt, orderCreatedIntegrationEvent.Id);

    //                        if (attempt >= MaxRetryAttempts)
    //                        {
    //                            await RouteToDeadLetterQueueAsync(orderCreatedIntegrationEvent, ex.Message);
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

    //    private async Task ProcessEventWithIdempotencyAsync(OrderCreatedIntegrationEvent orderCreatedIntegrationEvent, CancellationToken cancellationToken)
    //    {
    //        // Create a temporary scope when a message arrives
    //        using var scope = scopeFactory.CreateScope();
    //        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
    //        // Resolve your scoped ISender safely inside this block
    //        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

    //        // Check if the event was already processed
    //        var alreadyProcessed = await dbContext.Set<InboxMessage>()
    //            .AnyAsync(m => m.Id == orderCreatedIntegrationEvent.Id, cancellationToken);

    //        if (alreadyProcessed)
    //        {
    //            logger.LogWarning("Duplicate event detected. Skipping Event ID: {EventId}", orderCreatedIntegrationEvent.Id);
    //            return; // Return early without throwing an error (acknowledges message to Kafka)
    //        }

    //        using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
    //        try
    //        {
    //            // Add to Inbox to lock the ID
    //            var inboxMessage = new InboxMessage
    //            {
    //                Id = orderCreatedIntegrationEvent.Id,
    //                EventType = orderCreatedIntegrationEvent.EventType
    //            };

    //            dbContext.Set<InboxMessage>().Add(inboxMessage);
    //            await dbContext.SaveChangesAsync(cancellationToken);

    //            var command = orderCreatedIntegrationEvent.ToCreatePaymentCommand();

    //            //// Dispatch the command
    //            await sender.Send(command, cancellationToken);
    //            // Commit both steps simultaneously. (Write to Inbox and process the command)
    //            await transaction.CommitAsync(cancellationToken);

    //            logger.LogInformation("Successfully processed order created event.");
    //            logger.LogInformation("Integration Event handled: {IntegrationEvent}", GetType().Name);
    //        }
    //        catch (Exception ex)
    //        {
    //            await transaction.RollbackAsync(cancellationToken);
    //            logger.LogError(ex, "Failed to process order created event for User: {CustomerId}", orderCreatedIntegrationEvent.CustomerId);
    //            throw; // Throwing will prevent Kafka from committing the offset (per your consumer config)
    //        }
    //    }

    //    private async Task RouteToDeadLetterQueueAsync(OrderCreatedIntegrationEvent orderCreatedIntegrationEvent, string errorMessage)
    //    {
    //        var dlqTopic = $"{nameof(OrderCreatedIntegrationEvent)}-dlq";

    //        logger.LogCritical("MOVING TO DLQ: Event ID {EventId} failed after {MaxAttempts} attempts. Routing to topic: {DlqTopic}. Error: {Error}",
    //            orderCreatedIntegrationEvent.Id, MaxRetryAttempts, dlqTopic, errorMessage);

    //        // Creates a valid object that matches the IntegrationEvent constraint
    //        var dlqPayload = new DeadLetterQueueEvent
    //        {
    //            Id = orderCreatedIntegrationEvent.Id, // Reuses original event ID for traceability
    //            OccurredOn = orderCreatedIntegrationEvent.OccurredOn,
    //            OriginalEvent = orderCreatedIntegrationEvent,
    //            ExceptionMessage = errorMessage,
    //            FailedAtUtc = DateTime.UtcNow
    //        };

    //        // Publish to Kafka DLQ Topic
    //        await kafkaProducer.PublishAsync(dlqTopic, dlqPayload);
    //    }
    //}
}

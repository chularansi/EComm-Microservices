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
                catch (DbUpdateException dbEx)
                {
                    // If another replica processed this event at the exact same fraction of a second, 
                    // the database Primary Key constraint on InboxMessages.Id will trip and throw this error.
                    await transaction.RollbackAsync(cancellationToken);
                    logger.LogWarning("Concurrent message clash caught! Event ID {EventId} was saved elsewhere. Rolling back.", message.Id);

                    var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
                    logger.LogError("DbUpdateException caught: {Message}. Event ID: {EventId}", innerMessage, message.Id);
                    throw; // Re-throw so your KafkaConsumer worker knows NOT to commit the Kafka offset!
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
}


using Messaging.Events;
using Messaging.Kafka;
using Microsoft.Extensions.Logging;

namespace Ordering.Infrastructure.Messaging
{
    public class KeycloakUserIntegrationEventHandler(
        ApplicationDbContext dbContext,
        ILogger<KeycloakUserIntegrationEventHandler> logger)
        : IIntegrationEventHandler<KeycloakUserIntegrationEvent>
    {
        public async Task HandleAsync(KeycloakUserIntegrationEvent message, CancellationToken cancellationToken)
        {
            await Task.Yield();

            // 1. Idempotency Check (Inbox Pattern)
            bool alreadyProcessed = await dbContext.InboxMessages
                .AnyAsync(m => m.Id == message.Id, cancellationToken);

            if (alreadyProcessed)
            {
                logger.LogWarning("Duplicate Event {EventId} skipped.", message.Id);
                return;
            }

            var strategy = dbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    // 2. Stage Inbox Record to claim message ownership
                    var inboxEntry = new InboxMessage { Id = message.Id, EventType = message.ActionType };
                    await dbContext.InboxMessages.AddAsync(inboxEntry, cancellationToken);

                    var domainCustomerId = CustomerId.Of(message.CustomerId);

                    // 3. Route execution by parsing the incoming ActionType property
                    switch (message.ActionType)
                    {
                        case "USER_CREATED":
                            var newCustomer = Customer.Create(domainCustomerId, message.Name, message.Email);
                            await dbContext.Customers.AddAsync(newCustomer, cancellationToken);
                            logger.LogInformation("Synchronized new customer: {CustomerId}", message.CustomerId);
                            break;

                        case "USER_UPDATED":
                            var existingCustomer = await dbContext.Customers
                                .FirstOrDefaultAsync(c => c.Id == domainCustomerId, cancellationToken);

                            if (existingCustomer != null)
                            {
                                // Call your domain entity's update/mutation logic
                                existingCustomer.Update(message.Name, message.Email);
                                logger.LogInformation("Updated profile details for customer: {CustomerId}", message.CustomerId);
                            }
                            break;

                        case "USER_DELETED":
                            // Soft-delete or hard-delete depending on system compliance guidelines
                            var customerToDelete = await dbContext.Customers
                                .FirstOrDefaultAsync(c => c.Id == domainCustomerId, cancellationToken);

                            if (customerToDelete != null)
                            {
                                dbContext.Customers.Remove(customerToDelete);
                                logger.LogInformation("Purged removed profile from data engine: {CustomerId}", message.CustomerId);
                            }
                            break;

                        default:
                            logger.LogWarning("Unknown ActionType observed: {Type}", message.ActionType);
                            break;
                    }

                    await dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    logger.LogError(ex, "Failure in data processing loop for event {EventId}", message.Id);
                    throw; // Prevent Kafka offset commit
                }
            });
        }
    }
}

// What Your Architecture Looks Like Now

// Instead of writing custom C# registration endpoints, your event flow is completely automated:

// User Registers: A user signs up directly on the Keycloak user interface or via your
// frontend app calling Keycloak's APIs.

// Keycloak Fires Event: Keycloak automatically catches the registration internally and
// drops the JSON event directly onto your Kafka broker's KeycloakUserIntegrationEvent topic.

// Ordering Service Consumes: Your KeycloakUserIntegrationEventHandler in the
// Ordering service hears the message, deserializes Keycloak's nested properties using
// the mapping attributes, and seeds the Customers database table.

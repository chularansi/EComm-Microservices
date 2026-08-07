using Messaging.Events;
using Messaging.Kafka;

namespace Ordering.Application.Orders.CreateOrder
{
    public class OrderCreatedDomainEventHandler
        (IKafkaProducer producer, IApplicationDbContext dbContext, ILogger<OrderCreatedDomainEventHandler> logger)
        : INotificationHandler<OrderCreatedDomainEvent>
    {
        public async ValueTask Handle(OrderCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
        {
            try
            {
                // If the DB has an active transaction, publish after commit by using SaveChangesAsync continuation
                if (dbContext.Database.CurrentTransaction != null)
                {
                    // Wait for transaction to complete by saving changes, then publish
                    await dbContext.SaveChangesAsync(cancellationToken);
                }

                var integrationEvent = domainEvent.order.ToOrderCreatedIntegrationEvent();
                await producer.PublishAsync(nameof(OrderCreatedIntegrationEvent), integrationEvent);

                //logger.LogInformation("DIAGNOSTIC: OrderCreatedDomainEventHandler was successfully reached for Order ID: {OrderId}", domainEvent.order.Id);

                //var orderCreatedIntegrationEvent = domainEvent.order.ToOrderCreatedIntegrationEvent();

                //    await producer.PublishAsync(
                //        nameof(OrderCreatedIntegrationEvent),
                //        orderCreatedIntegrationEvent!
                //    );

                    logger.LogInformation("Successfully published OrderCreatedIntegrationEvent for Order: {OrderId}", domainEvent.order.Id);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to publish Kafka integration event for Order ID: {OrderId}", domainEvent.order.Id);
                throw; // Re-throw to let BuildingBlocks CQRS handle or log the pipeline failure
            }
        }
    }
}


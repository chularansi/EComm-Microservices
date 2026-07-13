namespace Ordering.Application.Orders.CreateOrder
{
    // IPublishEndpoint publishEndpoint, IFeatureManager featureManager, 
    public class CreateOrderEventHandler(ILogger<CreateOrderEventHandler> logger) : INotificationHandler<OrderCreatedEvent>
    {
        public ValueTask Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
        {
            logger.LogInformation("Domain Event handled: {DomainEvent}", notification.GetType().Name);
            return ValueTask.CompletedTask;
        }
    }
}

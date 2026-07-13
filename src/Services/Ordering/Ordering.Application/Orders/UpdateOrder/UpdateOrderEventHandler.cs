namespace Ordering.Application.Orders.UpdateOrder
{
    public class UpdateOrderEventHandler(ILogger<UpdateOrderEventHandler> logger) : INotificationHandler<OrderUpdatedEvent>
    {
        public ValueTask Handle(OrderUpdatedEvent notification, CancellationToken cancellationToken)
        {
            logger.LogInformation("Domain Event handled: {DomainEvent}", notification.GetType().Name);
            return ValueTask.CompletedTask;
        }
    }
}

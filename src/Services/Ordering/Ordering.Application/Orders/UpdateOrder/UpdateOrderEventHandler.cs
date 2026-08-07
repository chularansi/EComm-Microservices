namespace Ordering.Application.Orders.UpdateOrder
{
    public class UpdateOrderEventHandler(ILogger<UpdateOrderEventHandler> logger) : INotificationHandler<OrderUpdatedDomainEvent>
    {
        public ValueTask Handle(OrderUpdatedDomainEvent notification, CancellationToken cancellationToken)
        {
            logger.LogInformation("Domain Event handled: {DomainEvent}", notification.GetType().Name);
            return ValueTask.CompletedTask;
        }
    }
}

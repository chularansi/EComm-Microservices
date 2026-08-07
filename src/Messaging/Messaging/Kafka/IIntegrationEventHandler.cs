using Messaging.Events;

namespace Messaging.Kafka
{
    public interface IIntegrationEventHandler<T> where T : IntegrationEvent
    {
        Task HandleAsync(T @event, CancellationToken cancellationToken);
    }
}

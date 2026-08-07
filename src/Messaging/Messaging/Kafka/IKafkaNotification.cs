using BuildingBlocks.CQRS.Dispatcher;
using Messaging.Events;

namespace Messaging.Kafka
{
    // Similar to IDomainEvent, but for Kafka notifications. This interface is used to represent a notification that is sent via Kafka, and it contains the event data of type TEvent.
    // TEvent is constrained to be of type IntegrationEvent, which means that any class that implements this interface must provide an event data of type IntegrationEvent.
    // This interface is useful for decoupling the event data from the notification mechanism, allowing for more flexible and testable code.
    // The INotification interface is part of the BuildingBlocks library, which is used for implementing the mediator pattern in .NET applications. It allows for sending notifications to multiple handlers without the sender needing to know about the handlers.
    // The IKafkaNotification interface is a generic interface that takes a type parameter TEvent, which must be a subclass of IntegrationEvent. This allows for strong typing of the event data, ensuring that only valid event types can be used with this notification.
    // The EventData property is read-only and provides access to the event data associated with the notification. This allows handlers to access the event data when processing the notification.
    // Overall, the IKafkaNotification interface is a key part of the messaging infrastructure in a .NET application that uses Kafka for event-driven communication. It provides a standardized way to represent notifications that carry event data, enabling better separation of concerns and more maintainable code.

    public interface IKafkaNotification<TEvent> : INotification where TEvent : IntegrationEvent
    {
        TEvent EventData { get; }
    }
}

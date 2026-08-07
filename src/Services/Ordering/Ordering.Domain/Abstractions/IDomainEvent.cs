using BuildingBlocks.CQRS.Dispatcher;

namespace Ordering.Domain.Abstractions
{
    public interface IDomainEvent : INotification
    {
        Guid EventId => Guid.NewGuid();
        //private static Guid EventId
        //{
        //    get
        //    {
        //        return Guid.NewGuid();
        //    }
        //}
        public DateTime OccurredOn => DateTime.Now;
        public string EventType => GetType().AssemblyQualifiedName!;
    }
}

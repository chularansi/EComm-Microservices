namespace Ordering.Domain.Events
{
    public record OrderUpdatedDomainEvent(Order order) : IDomainEvent;
}

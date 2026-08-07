namespace Ordering.Infrastructure.Messaging
{
    public class InboxMessage
    {
        public Guid Id { get; set; }           // Maps to integrationEvent.Id
        public string EventType { get; set; } = default!;
        public DateTime ProcessedOnUtc { get; set; } = DateTime.UtcNow;
    }
}

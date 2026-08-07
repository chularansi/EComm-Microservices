namespace Messaging.Events
{
    public record DeadLetterQueueEvent : IntegrationEvent
    {
        public object OriginalEvent { get; init; } = default!;
        public string ExceptionMessage { get; init; } = default!;
        public DateTime FailedAtUtc { get; init; } = DateTime.UtcNow;
    }
}

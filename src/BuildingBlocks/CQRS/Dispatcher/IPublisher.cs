namespace BuildingBlocks.CQRS.Dispatcher
{
    public interface IPublisher
    {
        ValueTask Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification;
    }
}

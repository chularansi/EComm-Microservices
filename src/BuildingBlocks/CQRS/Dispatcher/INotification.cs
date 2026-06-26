namespace BuildingBlocks.CQRS.Dispatcher
{
    public interface INotification;

    public interface INotificationHandler<in TNotification>
        where TNotification : INotification
    {
        ValueTask Handle(TNotification notification, CancellationToken cancellationToken);
    }

    //public interface IPublisher
    //{
    //    ValueTask Publish<TNotification>(
    //        TNotification notification,
    //        CancellationToken cancellationToken = default)
    //        where TNotification : INotification;
    //}
}

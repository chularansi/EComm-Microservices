using System.Collections.Frozen;

namespace BuildingBlocks.CQRS.Dispatcher
{
    internal sealed class Dispatcher(
    IServiceProvider provider,
    DispatcherRegistry registry) : ISender, IPublisher
    {
        public ValueTask<TResponse> Send<TResponse>(
            IRequest<TResponse> request,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (!registry.RequestWrappers.TryGetValue(request.GetType(), out var wrapper))
            {
                throw new InvalidOperationException(
                    $"No handler registered for request type '{request.GetType().FullName}'.");
            }

            // Reference-type cast - cheap, no boxing.
            return ((RequestHandlerBase<TResponse>)wrapper).Handle(request, provider, cancellationToken);
        }

        public ValueTask Publish<TNotification>(
            TNotification notification,
            CancellationToken cancellationToken = default)
            where TNotification : INotification
        {
            ArgumentNullException.ThrowIfNull(notification);

            if (!registry.NotificationWrappers.TryGetValue(notification.GetType(), out var wrapper))
            {
                return ValueTask.CompletedTask;
            }

            return wrapper.Handle(notification, provider, cancellationToken);
        }
    }

    internal sealed class DispatcherRegistry(
        FrozenDictionary<Type, RequestHandlerBase> requestWrappers,
        FrozenDictionary<Type, NotificationHandlerBase> notificationWrappers)
    {
        public FrozenDictionary<Type, RequestHandlerBase> RequestWrappers { get; } = requestWrappers;
        public FrozenDictionary<Type, NotificationHandlerBase> NotificationWrappers { get; } = notificationWrappers;
    }
}

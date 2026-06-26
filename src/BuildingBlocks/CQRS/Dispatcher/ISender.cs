namespace BuildingBlocks.CQRS.Dispatcher
{
    public interface ISender
    {
        ValueTask<TResponse> Send<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default);
    }
}

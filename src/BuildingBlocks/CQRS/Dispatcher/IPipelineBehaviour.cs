namespace BuildingBlocks.CQRS.Dispatcher
{
    public delegate ValueTask<TResponse> RequestHandlerDelegate<TResponse>();

    public interface IPipelineBehaviour<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        ValueTask<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken);
    }
}

using BuildingBlocks.CQRS.Dispatcher;

namespace BuildingBlocks.CQRS
{
    public interface IQuery<out TResponse> : IRequest<TResponse>  
        where TResponse : notnull
    {
    }
}

namespace BuildingBlocks.CQRS.Dispatcher
{
    public interface IRequest<out TResponse>
    {}
    
    public interface IRequest : IRequest<Unit>
    {}

    public readonly struct Unit
    {
        public static readonly Unit Value = default;
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.CQRS.Dispatcher
{
    internal abstract class RequestHandlerBase;

    internal abstract class RequestHandlerBase<TResponse> : RequestHandlerBase
    {
        public abstract ValueTask<TResponse> Handle(
            IRequest<TResponse> request,
            IServiceProvider provider,
            CancellationToken cancellationToken);
    }

    internal sealed class RequestHandlerWrapper<TRequest, TResponse> : RequestHandlerBase<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override ValueTask<TResponse> Handle(
            IRequest<TResponse> request,
            IServiceProvider provider,
            CancellationToken cancellationToken)
        {
            var typed = (TRequest)request;
            var handler = provider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
            var behaviors = provider.GetServices<IPipelineBehaviour<TRequest, TResponse>>();

            // Build the pipeline: handler at the core, behaviors wrapped outside in registration order.
            // Iterating in reverse means the first registered behavior runs outermost.
            RequestHandlerDelegate<TResponse> pipeline = () => handler.Handle(typed, cancellationToken);

            foreach (var behavior in behaviors.Reverse())
            {
                var next = pipeline;
                var current = behavior;
                pipeline = () => current.Handle(typed, next, cancellationToken);
            }

            return pipeline();
        }
    }
}

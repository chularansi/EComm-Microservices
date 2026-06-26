using BuildingBlocks.CQRS.Dispatcher;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.CQRS.Behaviours
{
    public sealed class CachingBehaviour<TRequest, TResponse>(
    HybridCache cache,
    ILogger<CachingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehaviour<TRequest, TResponse>
    where TRequest : IRequest<TResponse>    
    {
        public async ValueTask<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            if (request is not ICacheable cacheable)
            {
                return await next();
            }

            var options = cacheable.Expiration is { } expiration
                ? new HybridCacheEntryOptions { Expiration = expiration }
                : null;

            return await cache.GetOrCreateAsync(
                cacheable.CacheKey,
                async ct =>
                {
                    logger.LogInformation("Cache miss for {Key}", cacheable.CacheKey);
                    return await next();
                },
                options,
                cancellationToken: cancellationToken);
        }
    }
}

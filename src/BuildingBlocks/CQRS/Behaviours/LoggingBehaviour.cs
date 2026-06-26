using BuildingBlocks.CQRS.Dispatcher;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BuildingBlocks.CQRS.Behaviours
{
    public sealed class LoggingBehaviour<TRequest, TResponse>(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    : IPipelineBehaviour<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        public async ValueTask<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            logger.LogInformation("Handling {RequestName}", requestName);

            var sw = Stopwatch.StartNew();
            try
            {
                var response = await next();
                sw.Stop();
                logger.LogInformation("Handled {RequestName} in {Elapsed}ms", requestName, sw.ElapsedMilliseconds);
                return response;
            }
            catch (Exception ex)
            {
                sw.Stop();
                logger.LogError(ex, "Handler {RequestName} threw after {Elapsed}ms", requestName, sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}

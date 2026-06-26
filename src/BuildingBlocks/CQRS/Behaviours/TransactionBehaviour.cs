//using BuildingBlocks.CQRS.Dispatcher;
//using Microsoft.Extensions.Logging;

//namespace BuildingBlocks.CQRS.Behaviours
//{
//    public interface ITransactional;

//    public sealed class TransactionBehaviour<TRequest, TResponse>(
//        AppDbContext db,
//        ILogger<TransactionBehaviour<TRequest, TResponse>> logger)
//        : IPipelineBehaviour<TRequest, TResponse>
//        where TRequest : IRequest<TResponse>
//    {
//        public async ValueTask<TResponse> Handle(
//            TRequest request,
//            RequestHandlerDelegate<TResponse> next,
//            CancellationToken cancellationToken)
//        {
//            if (request is not ITransactional)
//            {
//                return await next();
//            }

//            // The InMemory provider does not support real transactions - this is a no-op there.
//            // For SQL Server / Postgres / SQLite this opens, commits, or rolls back as expected.
//            if (!db.Database.IsInMemory())
//            {
//                await using var tx = await db.Database.BeginTransactionAsync(cancellationToken);
//                try
//                {
//                    var response = await next();
//                    await tx.CommitAsync(cancellationToken);
//                    return response;
//                }
//                catch
//                {
//                    logger.LogWarning("Rolling back transaction for {Request}", typeof(TRequest).Name);
//                    await tx.RollbackAsync(cancellationToken);
//                    throw;
//                }
//            }

//            return await next();
//        }
//    }
//}

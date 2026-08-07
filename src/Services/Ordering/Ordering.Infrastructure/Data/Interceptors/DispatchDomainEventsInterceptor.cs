using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Ordering.Infrastructure.Data.Interceptors
{
    public class DispatchDomainEventsInterceptor(IPublisher publisher) : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            //DispatchDomainEvents(eventData.Context).GetAwaiter().GetResult();
            //return base.SavingChanges(eventData, result);
            var savingChangesResult = base.SavingChanges(eventData, result);
            DispatchDomainEvents(eventData.Context).GetAwaiter().GetResult();
            return savingChangesResult;
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            var savingChangesAsyncResult = await base.SavingChangesAsync(eventData, result, cancellationToken);
            await DispatchDomainEvents(eventData.Context);
            return savingChangesAsyncResult;
        }

        public async Task DispatchDomainEvents(DbContext? context)
        {
            if (context == null) return;

            // Find all entity instances inheriting from AggregateRoot that contain staged domain events
            var aggregates = context.ChangeTracker
                .Entries<IAggregate>()
                .Where(a => a.Entity.DomainEvents.Any())
                .Select(a => a.Entity);

            var domainEvents = aggregates
                .SelectMany(a => a.DomainEvents)
                .ToList();

            // Clear the events so they don't get accidentally fired twice if SaveChanges is re-called
            aggregates.ToList().ForEach(a => a.ClearDomainEvents());

            foreach (var domainEvent in domainEvents)
                // Note: Since IDomainEvent implements INotification, MediatR will map this perfectly
                // FIX: Casting to 'dynamic' forces .NET to evaluate the true runtime type (OrderCreatedDomainEvent)
                // instead of treating it like the base 'IDomainEvent' interface type.
                await publisher.Publish((dynamic)domainEvent);
        }
    }
}

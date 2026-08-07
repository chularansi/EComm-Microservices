using Ordering.Application.Data;
using Ordering.Infrastructure.Messaging;
using System.Reflection;

namespace Ordering.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly IPublisher publisher;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IPublisher publisher)
            : base(options)
        {
            this.publisher = publisher;
        }

        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // Apply all configurations from the current assembly
            // This will automatically apply any IEntityTypeConfiguration<T> implementations found in the assembly
            builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
            base.OnModelCreating(builder);
        }

        //public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        //{
        //    // 1. Save data entities to the physical database FIRST 
        //    // This releases the row/table locks and ensures the Order ID actually exists!
        //    var result = await base.SaveChangesAsync(cancellationToken);

        //    // 2. Dispatch your custom internal domain events AFTER data is safely saved
        //    await DispatchDomainEventsAsync(cancellationToken);

        //    return result;
        //}

        //private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
        //{
        //    // Find all entity instances inheriting from AggregateRoot that contain staged domain events
        //    var domainEntities = ChangeTracker
        //        .Entries<IAggregate>()
        //        .Where(x => x.Entity.DomainEvents.Any())
        //        .ToList();

        //    var domainEvents = domainEntities
        //        .SelectMany(x => x.Entity.DomainEvents)
        //        .ToList();

        //    // Clear the events so they don't get accidentally fired twice if SaveChanges is re-called
        //    domainEntities.ForEach(x => x.Entity.ClearDomainEvents());

        //    // Publish each event through MediatR so OrderCreatedDomainEventHandler receives it
        //    foreach (var domainEvent in domainEvents)
        //    {
        //        // Note: Since IDomainEvent implements INotification, MediatR will map this perfectly
        //        // FIX: Casting to 'dynamic' forces .NET to evaluate the true runtime type (OrderCreatedDomainEvent)
        //        // instead of treating it like the base 'IDomainEvent' interface type.
        //        await publisher.Publish((dynamic)domainEvent, cancellationToken);
        //    }
        //}
    }
}

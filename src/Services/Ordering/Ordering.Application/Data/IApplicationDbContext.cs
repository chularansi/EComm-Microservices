using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Ordering.Application.Data
{
    public interface IApplicationDbContext
    {
        // Add this property to your interface signature
        DatabaseFacade Database { get; }

        DbSet<Customer> Customers { get; }
        DbSet<Product> Products { get; }
        DbSet<Order> Orders { get; }
        DbSet<OrderItem> OrderItems { get; }

        // We can add this property to support for InboxMessage entity in the future if needed
        // without using this DbSet<InboxMessage> InboxMessages { get; }
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}

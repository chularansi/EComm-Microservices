using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Application.Data;

namespace Ordering.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add infrastructure services here
            var connectionString = configuration.GetConnectionString("Database")
                ?? throw new InvalidOperationException("Connection string not found.");

            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseNpgsql(connectionString);
            });

            // This architectural pattern is called Dependency Inversion
            // Register the ApplicationDbContext as the implementation of IApplicationDbContext
            // This allows for dependency injection of IApplicationDbContext in other parts of the application
            // The scoped lifetime is appropriate for DbContext, as it should be created per request in web applications
            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
            // For Dapper, we need to register the IDbConnectionFactory as a singleton, since it will be used to create connections for Dapper queries
            services.AddSingleton<IDbConnectionFactory>(sp => new NpgsqlConnectionFactory(connectionString));

            return services;
        }
    }
}

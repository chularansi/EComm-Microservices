using BuildingBlocks.CQRS.Behaviours;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Ordering.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Add application services here
            services.AddDispatcher(Assembly.GetExecutingAssembly());
            services.AddPipelineBehavior(typeof(LoggingBehaviour<,>));
            services.AddPipelineBehavior(typeof(ValidationBehaviour<,>));
            return services;
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using System.Collections.Frozen;
using System.Reflection;

namespace BuildingBlocks.CQRS.Dispatcher
{
    public static class DispatcherRegistration
    {
        public static IServiceCollection AddDispatcher(this IServiceCollection services, Assembly assembly)
        {
            var requestWrappers = new Dictionary<Type, RequestHandlerBase>();
            var notificationWrappers = new Dictionary<Type, NotificationHandlerBase>();

            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || type.IsInterface) continue;

                foreach (var iface in type.GetInterfaces())
                {
                    if (!iface.IsGenericType) continue;
                    var def = iface.GetGenericTypeDefinition();

                    if (def == typeof(IRequestHandler<,>))
                    {
                        services.AddScoped(iface, type);

                        var args = iface.GetGenericArguments();
                        var requestType = args[0];
                        var responseType = args[1];

                        if (!requestWrappers.ContainsKey(requestType))
                        {
                            var wrapperType = typeof(RequestHandlerWrapper<,>)
                                .MakeGenericType(requestType, responseType);
                            requestWrappers[requestType] =
                                (RequestHandlerBase)Activator.CreateInstance(wrapperType)!;
                        }
                    }
                    // ... INotificationHandler branch omitted, see repo
                }
            }

            var registry = new DispatcherRegistry(
                requestWrappers.ToFrozenDictionary(),
                notificationWrappers.ToFrozenDictionary());

            services.AddSingleton(registry);
            services.AddScoped<Dispatcher>();
            services.AddScoped<ISender>(sp => sp.GetRequiredService<Dispatcher>());
            services.AddScoped<IPublisher>(sp => sp.GetRequiredService<Dispatcher>());

            return services;
        }

        public static IServiceCollection AddPipelineBehavior(
            this IServiceCollection services,
            Type openGenericBehaviorType)
        {
            services.AddScoped(typeof(IPipelineBehaviour<,>), openGenericBehaviorType);
            return services;
        }
    }
}

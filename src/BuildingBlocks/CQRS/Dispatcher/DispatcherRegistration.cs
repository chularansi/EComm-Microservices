using Microsoft.Extensions.DependencyInjection;
using System.Collections.Frozen;
using System.Reflection;

namespace BuildingBlocks.CQRS.Dispatcher
{
    public static class DispatcherRegistration
    {
        public static IServiceCollection AddDispatcher(this IServiceCollection services, params Assembly[] assemblies)
        {
            var requestWrappers = new Dictionary<Type, RequestHandlerBase>();
            var notificationWrappers = new Dictionary<Type, NotificationHandlerBase>();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsAbstract || type.IsInterface) continue;

                    foreach (var iface in type.GetInterfaces())
                    {
                        if (!iface.IsGenericType) continue;
                        var def = iface.GetGenericTypeDefinition();

                        // FIX: Check if the interface is IRequestHandler OR if it inherits from it (like your ICommandHandler does)
                        if (def == typeof(IRequestHandler<,>) || typeof(IRequestHandler<,>).IsAssignableFrom(def))
                        {
                            // Register the interface type to its concrete implementation inside .NET DI
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

                        // FIX: Apply the same assigning filter check safely to your notifications
                        if (def == typeof(INotificationHandler<>) || typeof(INotificationHandler<>).IsAssignableFrom(def))
                        {
                            services.AddScoped(iface, type);

                            var args = iface.GetGenericArguments();
                            var notificationType = args[0];

                            if (!notificationWrappers.ContainsKey(notificationType))
                            {
                                var wrapperType = typeof(NotificationHandlerWrapper<>)
                                    .MakeGenericType(notificationType);
                                notificationWrappers[notificationType] =
                                    (NotificationHandlerBase)Activator.CreateInstance(wrapperType)!;
                            }
                        }
                    }
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

    //public static class DispatcherRegistration
    //{
    //    // FIX: Change parameter from 'Assembly assembly' to 'params Assembly[] assemblies'
    //    public static IServiceCollection AddDispatcher(this IServiceCollection services, params Assembly[] assemblies)
    //    {
    //        var requestWrappers = new Dictionary<Type, RequestHandlerBase>();
    //        var notificationWrappers = new Dictionary<Type, NotificationHandlerBase>();

    //        // Loop through all provided assemblies
    //        foreach (var assembly in assemblies)
    //        {
    //            foreach (var type in assembly.GetTypes())
    //            {
    //                if (type.IsAbstract || type.IsInterface) continue;

    //                foreach (var iface in type.GetInterfaces())
    //                {
    //                    if (!iface.IsGenericType) continue;
    //                    var def = iface.GetGenericTypeDefinition();

    //                    if (def == typeof(IRequestHandler<,>))
    //                    {
    //                        services.AddScoped(iface, type);

    //                        var args = iface.GetGenericArguments();
    //                        var requestType = args[0];
    //                        var responseType = args[1];

    //                        if (!requestWrappers.ContainsKey(requestType))
    //                        {
    //                            var wrapperType = typeof(RequestHandlerWrapper<,>)
    //                                .MakeGenericType(requestType, responseType);
    //                            requestWrappers[requestType] =
    //                                (RequestHandlerBase)Activator.CreateInstance(wrapperType)!;
    //                        }
    //                    }

    //                    // FIX: Ensure your INotificationHandler block is running across all assemblies!
    //                    if (def == typeof(INotificationHandler<>))
    //                    {
    //                        services.AddScoped(iface, type);

    //                        var args = iface.GetGenericArguments();
    //                        var notificationType = args[0];

    //                        if (!notificationWrappers.ContainsKey(notificationType))
    //                        {
    //                            var wrapperType = typeof(NotificationHandlerWrapper<>)
    //                                .MakeGenericType(notificationType);
    //                            notificationWrappers[notificationType] =
    //                                (NotificationHandlerBase)Activator.CreateInstance(wrapperType)!;
    //                        }
    //                    }
    //                }
    //            }
    //        }

    //        // Build registries containing components from ALL layers
    //        var registry = new DispatcherRegistry(
    //            requestWrappers.ToFrozenDictionary(),
    //            notificationWrappers.ToFrozenDictionary());

    //        services.AddSingleton(registry);
    //        services.AddScoped<Dispatcher>();
    //        services.AddScoped<ISender>(sp => sp.GetRequiredService<Dispatcher>());
    //        services.AddScoped<IPublisher>(sp => sp.GetRequiredService<Dispatcher>());

    //        return services;
    //    }

    //    public static IServiceCollection AddPipelineBehavior(
    //        this IServiceCollection services,
    //        Type openGenericBehaviorType)
    //    {
    //        services.AddScoped(typeof(IPipelineBehaviour<,>), openGenericBehaviorType);
    //        return services;
    //    }
    //}

    //public static class DispatcherRegistration
    //{
    //    public static IServiceCollection AddDispatcher(this IServiceCollection services, Assembly assembly)
    //    {
    //        var requestWrappers = new Dictionary<Type, RequestHandlerBase>();
    //        var notificationWrappers = new Dictionary<Type, NotificationHandlerBase>();

    //        foreach (var type in assembly.GetTypes())
    //        {
    //            if (type.IsAbstract || type.IsInterface) continue;

    //            foreach (var iface in type.GetInterfaces())
    //            {
    //                if (!iface.IsGenericType) continue;
    //                var def = iface.GetGenericTypeDefinition();

    //                if (def == typeof(IRequestHandler<,>))
    //                {
    //                    services.AddScoped(iface, type);

    //                    var args = iface.GetGenericArguments();
    //                    var requestType = args[0];
    //                    var responseType = args[1];

    //                    if (!requestWrappers.ContainsKey(requestType))
    //                    {
    //                        var wrapperType = typeof(RequestHandlerWrapper<,>)
    //                            .MakeGenericType(requestType, responseType);
    //                        requestWrappers[requestType] =
    //                            (RequestHandlerBase)Activator.CreateInstance(wrapperType)!;
    //                    }
    //                }
    //                // ... INotificationHandler branch omitted, see repo
    //            }
    //        }

    //        var registry = new DispatcherRegistry(
    //            requestWrappers.ToFrozenDictionary(),
    //            notificationWrappers.ToFrozenDictionary());

    //        services.AddSingleton(registry);
    //        services.AddScoped<Dispatcher>();
    //        services.AddScoped<ISender>(sp => sp.GetRequiredService<Dispatcher>());
    //        services.AddScoped<IPublisher>(sp => sp.GetRequiredService<Dispatcher>());

    //        return services;
    //    }

    //    public static IServiceCollection AddPipelineBehavior(
    //        this IServiceCollection services,
    //        Type openGenericBehaviorType)
    //    {
    //        services.AddScoped(typeof(IPipelineBehaviour<,>), openGenericBehaviorType);
    //        return services;
    //    }
    //}
}

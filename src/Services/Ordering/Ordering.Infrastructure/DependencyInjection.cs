using Confluent.Kafka;
using Messaging.Events;
using Messaging.Kafka;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ordering.Application.Data;
using Ordering.Infrastructure.Messaging;

namespace Ordering.Infrastructure
{
    public static class DependencyInjection
    {
        //public static async Task<IServiceCollection> AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)

        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add infrastructure services here
            var connectionString = configuration.GetConnectionString("Database")
                ?? throw new InvalidOperationException("Connection string not found.");

            // First: Mutate entity tracking properties
            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            // Second: Extract and dispatch domain events
            services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                //options.UseNpgsql(connectionString);
                options.UseNpgsql(connectionString, providerOptions =>
                {
                    providerOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null
                    );
                });
            });

            // This architectural pattern is called Dependency Inversion
            // Register the ApplicationDbContext as the implementation of IApplicationDbContext
            // This allows for dependency injection of IApplicationDbContext in other parts of the application
            // The scoped lifetime is appropriate for DbContext, as it should be created per request in web applications
            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

            // ==========================================
            // Kafka Producer | Consumer Configuration 
            // ==========================================

            // 1. Core Config Bindings
            var kafkaProducerConfig = configuration.GetSection("Kafka:Producer").Get<ProducerConfig>() ?? new ProducerConfig();
            var kafkaConsumerConfig = configuration.GetSection("Kafka:Consumer").Get<ConsumerConfig>() ?? new ConsumerConfig();

            services.AddSingleton(kafkaProducerConfig);
            services.AddSingleton(kafkaConsumerConfig);

            // 2. Concrete Implementation Registrations
            services.AddSingleton<IKafkaProducer, KafkaProducer>();
            services.AddSingleton<IKafkaConsumer, KafkaConsumer>();

            // ==========================================
            // Basket Checkout Worker Configuration
            // ==========================================
            var basketConsumerConfig = new ConsumerConfig(kafkaConsumerConfig)
            {
                GroupId = "basket-checkout-consumer-group",
                MaxPollIntervalMs = 3600000
            };

            services.AddScoped<IIntegrationEventHandler<BasketCheckoutIntegrationEvent>, BasketCheckoutIntegrationEventHandler>();

            // Use ActivatorUtilities or pass factory instances correctly using the service provider
            services.AddHostedService(sp =>
            {
                // Construct the isolated consumer for this specific background loop safely
                var consumerInstance = new KafkaConsumer(basketConsumerConfig);

                return new KafkaConsumerWorker<BasketCheckoutIntegrationEvent>(
                    consumerInstance,
                    sp.GetRequiredService<IKafkaProducer>(), // Resolve safely from container
                    "BasketCheckoutIntegrationEvent", // Ensure this matches producer topic string exactly
                    sp.GetRequiredService<IServiceScopeFactory>(),
                    sp.GetRequiredService<ILogger<KafkaConsumerWorker<BasketCheckoutIntegrationEvent>>>()
                );
            });

            // ==========================================
            // Customer Created Worker Configuration
            // ==========================================

            var customerConsumerConfig = new ConsumerConfig(kafkaConsumerConfig)
            {
                GroupId = "customer-managing-consumer-group",
                MaxPollIntervalMs = 3600000
            };

            // Register your handler to the scoped DI container
            services.AddScoped<IIntegrationEventHandler<KeycloakUserIntegrationEvent>, KeycloakUserIntegrationEventHandler>();

            // Append the background loop worker to the hosted services collection
            services.AddHostedService(sp =>
            {
                var consumerInstance = new KafkaConsumer(customerConsumerConfig);

                return new KafkaConsumerWorker<KeycloakUserIntegrationEvent>(
                    consumerInstance,
                    sp.GetRequiredService<IKafkaProducer>(),
                    "KeycloakUserIntegrationEvent", // Ensure your Identity/User service publishes to this exact topic name string
                    sp.GetRequiredService<IServiceScopeFactory>(),
                    sp.GetRequiredService<ILogger<KafkaConsumerWorker<KeycloakUserIntegrationEvent>>>()
                );
            });

            // ==========================================
            // Order Created Worker Configuration
            // ==========================================
            var orderConsumerConfig = new ConsumerConfig(kafkaConsumerConfig)
            {
                GroupId = "order-created-consumer-group",
                MaxPollIntervalMs = 3600000
            };

            services.AddScoped<IIntegrationEventHandler<OrderCreatedIntegrationEvent>, OrderCreatedIntegrationEventHandler>();

            services.AddHostedService(sp =>
            {
                var consumerInstance = new KafkaConsumer(orderConsumerConfig);

                return new KafkaConsumerWorker<OrderCreatedIntegrationEvent>(
                    consumerInstance,
                    sp.GetRequiredService<IKafkaProducer>(), // Resolve safely from container
                    "OrderCreatedIntegrationEvent",
                    sp.GetRequiredService<IServiceScopeFactory>(),
                    sp.GetRequiredService<ILogger<KafkaConsumerWorker<OrderCreatedIntegrationEvent>>>()
                );
            });

            return services;
        }
    }
}

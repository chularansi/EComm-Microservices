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
            // Kafka Producer Configuration 
            // ==========================================

            var kafkaProducerConfig = configuration
                .GetSection("Kafka:Producer")
                .Get<ProducerConfig>() ?? new ProducerConfig();

            services.AddSingleton(kafkaProducerConfig);
            services.AddSingleton<IKafkaProducer, KafkaProducer>();

            var kafkaConsumerConfig = configuration
                .GetSection("Kafka:Consumer")
                .Get<ConsumerConfig>() ?? new ConsumerConfig();

            // ==========================================
            // Basket Checkout Worker Configuration
            // ==========================================

            var basketConsumerConfig = new ConsumerConfig(kafkaConsumerConfig)
            {
                GroupId = "basket-checkout-consumer-group", // Keeps groups isolated
                MaxPollIntervalMs = 3600000 // 1 Hour window for debugging breakpoints!
            };

            // Register your handler as Scoped (or Transient)! It can now safely use DbContext
            services.AddScoped<IIntegrationEventHandler<BasketCheckoutIntegrationEvent>, BasketCheckoutIntegrationEventHandler>();

            services.AddHostedService(sp => new KafkaConsumerWorker<BasketCheckoutIntegrationEvent>(
                new KafkaConsumer(basketConsumerConfig),
                new KafkaProducer(kafkaProducerConfig),
                nameof(BasketCheckoutIntegrationEvent),
                sp.GetRequiredService<IServiceScopeFactory>(), // Pass the factory here
                sp.GetRequiredService<ILogger<KafkaConsumerWorker<BasketCheckoutIntegrationEvent>>>()
            ));

            // ==========================================
            // Order Created Worker Configuration
            // ==========================================

            var orderConsumerConfig = new ConsumerConfig(kafkaConsumerConfig)
            {
                GroupId = "order-created-consumer-group",
                MaxPollIntervalMs = 3600000 // 1 Hour window for debugging breakpoints!
            };

            // Register your handler as Scoped (or Transient)!
            services.AddScoped<IIntegrationEventHandler<OrderCreatedIntegrationEvent>, OrderCreatedIntegrationEventHandler>();

            services.AddHostedService(sp => new KafkaConsumerWorker<OrderCreatedIntegrationEvent>(
                new KafkaConsumer(orderConsumerConfig),
                new KafkaProducer(kafkaProducerConfig),
                nameof(OrderCreatedIntegrationEvent),
                sp.GetRequiredService<IServiceScopeFactory>(), // Pass the factory here
                sp.GetRequiredService<ILogger<KafkaConsumerWorker<OrderCreatedIntegrationEvent>>>()
            ));

            var bootstrapServers = kafkaProducerConfig.BootstrapServers
                ?? throw new InvalidOperationException("Kafka bootstrap servers not configured.");

            //await KafkaTopicProvisioner.EnsureTopicsExistAsync(
            //    bootstrapServers,
            //    "BasketCheckoutIntegrationEvent",
            //    "OrderCreatedIntegrationEvent"
            //);

            return services;
        }
    }
}

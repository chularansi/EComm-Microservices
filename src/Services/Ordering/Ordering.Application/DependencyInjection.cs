using BuildingBlocks.CQRS.Behaviours;
using Confluent.Kafka;
using Messaging.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Ordering.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(
            this IServiceCollection services, IConfiguration configuration)
        {
            // Add application services here
            //services.AddDispatcher(Assembly.GetExecutingAssembly());
            //services.AddPipelineBehavior(typeof(LoggingBehaviour<,>));
            //services.AddPipelineBehavior(typeof(ValidationBehaviour<,>));

            // 1. Bind the Producer configuration
            var producerConfig = configuration
                .GetSection("Kafka:Producer")
                .Get<ProducerConfig>();

            // 2. Bind the Consumer configuration
            var consumerConfig = configuration
                .GetSection("Kafka:Consumer")
                .Get<ConsumerConfig>();

            // 3. Register them as Singletons in the DI container so your services can use them
            //services.AddSingleton(producerConfig!);
            services.AddSingleton(consumerConfig!);
            services.AddSingleton<IKafkaProducer, KafkaProducer>();
            services.AddSingleton<IKafkaConsumer, KafkaConsumer>();

            services.AddFeatureManagement();
            return services;
        }
    }
}

using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace Messaging.Kafka
{
    public static class KafkaTopicProvisioner
    {
        public static async Task EnsureTopicsExistAsync(string bootstrapServers, params string[] topics)
        {
            var config = new AdminClientConfig { BootstrapServers = bootstrapServers };
            using var adminClient = new AdminClientBuilder(config).Build();

            try
            {
                // Fetch metadata to check what topics already exist on the broker
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

                var topicsToCreate = topics
                    .Where(topic => !metadata.Topics.Any(t => t.Topic == topic))
                    .Select(topic => new TopicSpecification
                    {
                        Name = topic,
                        NumPartitions = 2, // Matches local dev performance
                        ReplicationFactor = 1
                    })
                    .ToList();

                if (topicsToCreate.Any())
                {
                    await adminClient.CreateTopicsAsync(topicsToCreate);
                    foreach (var topic in topicsToCreate)
                    {
                        Console.WriteLine($"[Kafka] Pre-provisioned missing topic: {topic.Name}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Kafka Warning] Failed to pre-provision topics: {ex.Message}. Relying on fallback auto-creation.");
            }
        }
    }
}

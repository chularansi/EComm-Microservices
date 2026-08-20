using System.Text.Json.Serialization;

namespace Messaging.Events
{
    // Rename or widen the model scope to represent all User actions
    public record KeycloakUserIntegrationEvent : IntegrationEvent
    {
        // Tells the engine if this specific frame is a CREATE, UPDATE, or DELETE
        [JsonPropertyName("eventType")]
        public string ActionType { get; init; } = default!;

        [JsonPropertyName("userId")]
        public Guid CustomerId { get; init; }

        [JsonPropertyName("details")]
        public KeycloakDetails? InnerDetails { get; init; }

        [JsonIgnore]
        public string Name => InnerDetails != null
            ? $"{InnerDetails.First_Name} {InnerDetails.Last_Name}".Trim()
            : string.Empty;

        [JsonIgnore]
        public string Email => InnerDetails?.Email ?? string.Empty;
    }

    public record KeycloakDetails(
        [property: JsonPropertyName("first_name")] string First_Name,
        [property: JsonPropertyName("last_name")] string Last_Name,
        [property: JsonPropertyName("email")] string Email
    );
}

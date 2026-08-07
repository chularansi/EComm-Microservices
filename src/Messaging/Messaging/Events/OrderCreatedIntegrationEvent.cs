namespace Messaging.Events
{
    public record OrderCreatedIntegrationEvent(
        Guid Id,
        Guid CustomerId,
        string OrderName,
        AddressIntegrationDto ShippingAddress,
        AddressIntegrationDto BillingAddress,
        PaymentIntegrationDto Payment,
        string Status, // Kept as string to prevent enum sharing issues across services
        List<OrderItemIntegrationDto> OrderItems
    ) : IntegrationEvent;

    // Supporting data structures
    public record AddressIntegrationDto(
        string FirstName,
        string LastName,
        string EmailAddress,
        string AddressLine,
        string Country,
        string State,
        string ZipCode
    );

    public record PaymentIntegrationDto(
        string CardName,
        string CardNumber,
        string Expiration,
        string CVV,
        int PaymentMethod
    );

    public record OrderItemIntegrationDto(
        Guid OrderId,
        Guid ProductId,
        int Quantity,
        decimal Price
    );
}

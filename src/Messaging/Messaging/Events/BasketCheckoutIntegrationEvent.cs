namespace Messaging.Events
{
    public record BasketItemIntegrationEventDto
    {
        public Guid ProductId { get; set; } = default!;
        public string ProductName { get; set; } = default!;
        public int Quantity { get; set; } = default!;
        public decimal Price { get; set; } = default!;
    }

    public record AddressIntegrationEventDto
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string EmailAddress { get; set; } = default!;
        public string AddressLine { get; set; } = default!;
        public string Country { get; set; } = default!;
        public string State { get; set; } = default!;
        public string ZipCode { get; set; } = default!;
    };

    public record PaymentInfoIntegrationEventDto
    {
        public string CardName { get; set; } = default!;
        public string CardNumber { get; set; } = default!;
        public string Expiration { get; set; } = default!;
        public string Cvv { get; set; } = default!;
        public int PaymentMethod { get; set; } = default!;
    };

    public record BasketCheckoutIntegrationEvent : IntegrationEvent
    {
        public string UserName { get; set; } = default!;
        public Guid CustomerId { get; set; } = default!;
        public decimal TotalPrice { get; set; } = default!;
        public List<BasketItemIntegrationEventDto> Items { get; set; } = [];
        public AddressIntegrationEventDto ShippingAddress { get; set; } = default!;
        public bool ShippingEqualsBilling { get; set; } // Flag toggled by frontend
        public AddressIntegrationEventDto BillingAddress { get; set; } = default!;
        public PaymentInfoIntegrationEventDto PaymentInfo { get; set; } = default!;
    }
}

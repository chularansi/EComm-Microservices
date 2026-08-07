using System.ComponentModel.DataAnnotations;

namespace Basket.API.Basket.CheckoutBasket
{
    public record CheckoutBasketRequest(BasketCheckoutDto BasketCheckoutDto);
    public record CheckoutBasketCommand(BasketCheckoutDto BasketCheckoutDto) : ICommand<CheckoutBasketResult>;
    public record CheckoutBasketResult(bool IsSuccess);
    public record CheckoutBasketResponse(bool IsSuccess);

    public record BasketItemDto
    {
        public Guid ProductId { get; set; } = default!;
        public string ProductName { get; set; } = default!;
        public int Quantity { get; set; } = default!;
        public decimal Price { get; set; } = default!;
    }

    public record AddressDto
    {
        public string FirstName { get; set; } = default!;
        public string LastName { get; set; } = default!;
        public string EmailAddress { get; set; } = default!;
        public string AddressLine { get; set; } = default!;
        public string Country { get; set; } = default!;
        public string State { get; set; } = default!;
        public string ZipCode { get; set; } = default!;
    };

    public record PaymentInfoDto
    {
        public string CardName { get; set; } = default!;
        public string CardNumber { get; set; } = default!;
        public string Expiration { get; set; } = default!;
        public string Cvv { get; set; } = default!;
        public int PaymentMethod { get; set; } = default!;
    };

    public class BasketCheckoutDto
    {
        public string UserName { get; set; } = default!;
        public Guid CustomerId { get; set; } = default!;
        public decimal TotalPrice { get; set; } = default!;
        public List<BasketItemDto> Items { get; set; } = [];
        public AddressDto ShippingAddress { get; set; } = default!;
        public bool ShippingEqualsBilling { get; set; } = default!; // Flag toggled by frontend
        public AddressDto BillingAddress { get; set; } = default!;
        public PaymentInfoDto PaymentInfo { get; set; } = default!;
    }
}

using Messaging.Events;

namespace Basket.API.Basket.CheckoutBasket
{
    public static class CheckoutBasketMappers
    {
        public static CheckoutBasketCommand ToCheckoutBasketCommand(this CheckoutBasketRequest request)
        {
            return new CheckoutBasketCommand(request.BasketCheckoutDto);
        }
        public static CheckoutBasketResponse ToCheckoutBasketResponse(this CheckoutBasketResult result)
        {
            return new CheckoutBasketResponse(result.IsSuccess);
        }
        public static BasketCheckoutIntegrationEvent ToBasketCheckoutIntegrationEvent(this BasketCheckoutDto dto)
        {
            return new BasketCheckoutIntegrationEvent
            {
                UserName = dto.UserName,
                CustomerId = dto.CustomerId,
                TotalPrice = dto.TotalPrice,
                Items = dto.Items.Select(i => new BasketItemIntegrationEventDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    Price = i.Price,
                }).ToList()!,
                ShippingAddress = new AddressIntegrationEventDto
                {
                    FirstName = dto.ShippingAddress.FirstName,
                    LastName = dto.ShippingAddress.LastName,
                    EmailAddress = dto.ShippingAddress.EmailAddress,
                    AddressLine = dto.ShippingAddress.AddressLine,
                    Country = dto.ShippingAddress.Country,
                    State = dto.ShippingAddress.State,
                    ZipCode = dto.ShippingAddress.ZipCode
                },
                BillingAddress = new AddressIntegrationEventDto
                {
                    FirstName = dto.BillingAddress.FirstName,
                    LastName = dto.BillingAddress.LastName,
                    EmailAddress = dto.BillingAddress.EmailAddress,
                    AddressLine = dto.BillingAddress.AddressLine,
                    Country = dto.BillingAddress.Country,
                    State = dto.BillingAddress.State,
                    ZipCode = dto.BillingAddress.ZipCode
                },
                PaymentInfo = new PaymentInfoIntegrationEventDto
                {
                    CardName = dto.PaymentInfo.CardName,
                    CardNumber = dto.PaymentInfo.CardNumber,
                    Expiration = dto.PaymentInfo.Expiration,
                    Cvv = dto.PaymentInfo.Cvv,
                    PaymentMethod = dto.PaymentInfo.PaymentMethod
                }
            };
        }
    }
}

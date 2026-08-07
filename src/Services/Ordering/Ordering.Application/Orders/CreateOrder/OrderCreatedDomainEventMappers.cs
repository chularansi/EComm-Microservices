using Messaging.Events;
using Ordering.Domain.Enums;

namespace Ordering.Application.Orders.CreateOrder
{
    public static class OrderCreatedDomainEventMappers
    {
        public static OrderCreatedIntegrationEvent ToOrderCreatedIntegrationEvent(this Order order)
        {
            var shippingDto = new AddressIntegrationDto(
                order.ShippingAddress.FirstName,
                order.ShippingAddress.LastName,
                order.ShippingAddress.EmailAddress!,
                order.ShippingAddress.AddressLine,
                order.ShippingAddress.Country,
                order.ShippingAddress.State,
                order.ShippingAddress.ZipCode
            );

            var billingDto = new AddressIntegrationDto(
                order.BillingAddress.FirstName,
                order.BillingAddress.LastName,
                order.BillingAddress.EmailAddress!,
                order.BillingAddress.AddressLine,
                order.BillingAddress.Country,
                order.BillingAddress.State,
                order.BillingAddress.ZipCode
            );

            var paymentDto = new PaymentIntegrationDto(
                order.Payment.CardName!,
                order.Payment.CardNumber,
                order.Payment.Expiration,
                order.Payment.CVV,
                order.Payment.PaymentMethod
            );

            // Map read-only Domain OrderItems to Integration Event DTOs
            var itemDtos = order.OrderItems.Select(item => new OrderItemIntegrationDto(
                OrderId: order.Id.Value,
                ProductId: item.ProductId.Value,
                Quantity: item.Quantity,
                Price: item.Price
            )).ToList();

            return new OrderCreatedIntegrationEvent(
                Id: order.Id.Value,
                CustomerId: order.CustomerId.Value,
                OrderName: order.OrderName.Value,
                ShippingAddress: shippingDto,
                BillingAddress: billingDto,
                Payment: paymentDto,
                Status: order.Status?.ToString() ?? nameof(OrderStatus.Pending), // Clean string decoupling!
                OrderItems: itemDtos
            );
        }
    }
}

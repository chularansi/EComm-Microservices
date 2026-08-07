using Messaging.Events;
using Ordering.Application.Dtos;
using Ordering.Application.Orders.CreateOrder;
using Ordering.Domain.Enums;

namespace Ordering.Infrastructure.Messaging
{
    public static class BasketCheckoutIntegrationEventMappers
    {
        public static CreateOrderCommand ToCreateOrderCommand(this BasketCheckoutIntegrationEvent message)
        {
            // Create full order with incoming event data
            var shippingAddressDto = new AddressDto(
                FirstName: message.ShippingAddress.FirstName,
                LastName: message.ShippingAddress.LastName,
                EmailAddress: message.ShippingAddress.EmailAddress,
                AddressLine: message.ShippingAddress.AddressLine,
                Country: message.ShippingAddress.Country,
                State: message.ShippingAddress.State,
                ZipCode: message.ShippingAddress.ZipCode
            );
            var billingAddressDto = new AddressDto(
                FirstName: message.BillingAddress.FirstName,
                LastName: message.BillingAddress.LastName,
                EmailAddress: message.BillingAddress.EmailAddress,
                AddressLine: message.BillingAddress.AddressLine,
                Country: message.BillingAddress.Country,
                State: message.BillingAddress.State,
                ZipCode: message.BillingAddress.ZipCode
            );

            var paymentDto = new PaymentDto(
                CardName: message.PaymentInfo.CardName,
                CardNumber: message.PaymentInfo.CardNumber,
                Expiration: message.PaymentInfo.Expiration,
                CVV: message.PaymentInfo.Cvv,
                PaymentMethod: message.PaymentInfo.PaymentMethod
            );

            var orderId = Guid.NewGuid();

            var orderItemsDto = message.Items.Select(item => new OrderItemDto(
                OrderId: orderId,
                ProductId: item.ProductId,
                Quantity: item.Quantity,
                Price: item.Price
            )).ToList();

            var orderDto = new OrderDto(
                Id: orderId,
                CustomerId: message.CustomerId,
                OrderName: message.UserName, // OrderName: $"Order_{message.UserName}_{DateTime.UtcNow:yyyyMMddHHmmss}"
                ShippingAddress: shippingAddressDto,
                BillingAddress: billingAddressDto,
                Payment: paymentDto,
                Status: OrderStatus.Pending,
                OrderItems: orderItemsDto
                //OrderItems: // Hardcoded order items for testing purposes
                //[
                //    new OrderItemDto(orderId, new Guid("5334c996-8457-4cf0-815c-ed2b77c4ff61"), 2, 500),
                //    new OrderItemDto(orderId, new Guid("c67d6323-e8b1-4bdf-9a75-b0d0d2e7e914"), 1, 400)
                //]
            );

            return new CreateOrderCommand(orderDto);
        }
    }
}

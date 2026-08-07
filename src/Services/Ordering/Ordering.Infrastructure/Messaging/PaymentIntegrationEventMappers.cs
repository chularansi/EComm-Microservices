using Messaging.Events;

namespace Ordering.Infrastructure.Messaging
{
    public record CreatePaymentCommand(
        Guid OrderId,
        Guid CustomerId,
        decimal Amount,
        string CardName,
        string CardNumber,
        string Expiration,
        string CVV,
        int PaymentMethod
    ) : IRequest; // Here, should use ICommand<CreatePaymentResult> instead of IRequest if you have a specific response type for the command.

    public static class PaymentIntegrationEventMappers
    {
        public static CreatePaymentCommand ToCreatePaymentCommand(this OrderCreatedIntegrationEvent message)
        {
            return new CreatePaymentCommand(
                OrderId: message.Id,
                CustomerId: message.CustomerId,
                Amount: message.OrderItems.Sum(item => item.Price * item.Quantity),
                CardName: message.Payment.CardName,
                CardNumber: message.Payment.CardNumber,
                Expiration: message.Payment.Expiration,
                CVV: message.Payment.CVV,
                PaymentMethod: message.Payment.PaymentMethod
            );
        }
    }
}

using Messaging.Events;
using Messaging.Kafka;

namespace Basket.API.Basket.CheckoutBasket
{
    public class CheckoutBasketCommandHandler
        (IBasketRepository repository, IKafkaProducer producer) 
        : ICommandHandler<CheckoutBasketCommand, CheckoutBasketResult>
    {
        public async ValueTask<CheckoutBasketResult> Handle(CheckoutBasketCommand command, CancellationToken cancellationToken)
        {
            // get existing basket with total price
            // Set totalprice on basketcheckout event message
            // send basket checkout event to kafka
            // delete the basket

            var basket = await repository.GetBasket(command.BasketCheckoutDto.UserName, cancellationToken);
            if (basket == null)
            {
                return new CheckoutBasketResult(false);
            }

            var eventMessage = command.BasketCheckoutDto.ToBasketCheckoutIntegrationEvent();
            eventMessage.TotalPrice = basket.TotalPrice;

            // Map items from your repository basket into the Kafka event message
            eventMessage.Items = basket.Items.Select(item => new BasketItemIntegrationEventDto
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                Price = item.Price
            }).ToList()!;

            // Use the correct method and parameters as per IKafkaProducer interface
            await producer.PublishAsync(nameof(BasketCheckoutIntegrationEvent), eventMessage);

            await repository.DeleteBasket(command.BasketCheckoutDto.UserName, cancellationToken);

            return new CheckoutBasketResult(true);
        }
    }
}

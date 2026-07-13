using Ordering.Application.Orders.CreateOrder;

namespace Ordering.API.Order.CreateOrder
{
    public static class CreateOrderMappers
    {
        public static CreateOrderCommand ToCreateOrderCommand(this CreateOrderRequest request)
        {
            return new CreateOrderCommand(request.Order);
        }
        public static CreateOrderResponse ToCreateOrderResponse(this CreateOrderResult result)
        {
            return new CreateOrderResponse(result.Id);
        }
    }
}

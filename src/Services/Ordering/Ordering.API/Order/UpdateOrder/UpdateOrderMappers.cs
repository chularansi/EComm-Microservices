using Ordering.Application.Orders.UpdateOrder;

namespace Ordering.API.Order.UpdateOrder
{
    public static class UpdateOrderMappers
    {
        public static UpdateOrderCommand ToUpdateOrderCommand(this UpdateOrderRequest request)
        {
            return new UpdateOrderCommand(request.Order);
        }

        public static UpdateOrderResponse ToUpdateOrderResponse(this UpdateOrderResult result)
        {
            return new UpdateOrderResponse(result.IsSuccess);
        }
    }
}

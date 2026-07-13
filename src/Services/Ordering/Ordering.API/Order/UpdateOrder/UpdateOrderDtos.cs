using Ordering.Application.Dtos;

namespace Ordering.API.Order.UpdateOrder
{
    public record UpdateOrderRequest(OrderDto Order);
    public record UpdateOrderResponse(bool IsSuccess);
}

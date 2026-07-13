using Ordering.Application.Dtos;

namespace Ordering.API.Order.CreateOrder
{
    public record CreateOrderRequest(OrderDto Order);
    public record CreateOrderResponse(Guid Id);
}

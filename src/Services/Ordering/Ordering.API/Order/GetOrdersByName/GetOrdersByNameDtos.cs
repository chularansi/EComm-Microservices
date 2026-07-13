using Ordering.Application.Dtos;

namespace Ordering.API.Order.GetOrdersByName
{
    public record GetOrdersByNameRequest(string Name);
    public record GetOrdersByNameResponse(IEnumerable<OrderDto> Orders);
}

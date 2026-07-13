using Ordering.Application.Dtos;

namespace Ordering.API.Order.GetOrders
{
    public record GetOrdersRequest(PaginationRequest PaginationRequest);
    public record GetOrdersResponse(PaginatedResult<OrderDto> Orders);
}

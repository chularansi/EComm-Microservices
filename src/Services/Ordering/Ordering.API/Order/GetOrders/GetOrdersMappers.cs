using Ordering.Application.Orders.GetOrders;

namespace Ordering.API.Order.GetOrders
{
    public static class GetOrdersMappers
    {
        public static GetOrdersRequest ToGetOrdersRequest(this GetOrdersQuery query)
        {
            return new GetOrdersRequest(query.PaginationRequest);
        }

        public static GetOrdersResponse ToGetOrdersResponse(this GetOrdersResult result)
        {
            return new GetOrdersResponse(result.Orders);
        }
    }
}

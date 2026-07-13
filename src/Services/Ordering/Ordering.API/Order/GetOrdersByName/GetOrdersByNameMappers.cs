using Ordering.Application.Orders.GetOrdersByName;

namespace Ordering.API.Order.GetOrdersByName
{
    public static class GetOrdersByNameMappers
    {
        //public static GetOrdersByNameRequest ToGetOrdersByNameRequest(this string name)
        //{
        //    return new GetOrdersByNameRequest(name);
        //}

        public static GetOrdersByNameResponse ToGetOrdersByNameResponse(this GetOrdersByNameResult result)
        {
            return new GetOrdersByNameResponse(result.Orders);
        }
    }
}

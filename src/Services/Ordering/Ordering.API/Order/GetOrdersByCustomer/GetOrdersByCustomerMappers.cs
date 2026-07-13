using Ordering.Application.Orders.GetOrdersByCustomer;

namespace Ordering.API.Order.GetOrdersByCustomer
{
    public static class GetOrdersByCustomerMappers
    {
        //public static GetOrdersByCustomerRequest ToGetOrdersByCustomerRequest(this Guid customerId)
        //{
        //    return new GetOrdersByCustomerRequest(customerId);
        //}

        public static GetOrdersByCustomerResponse ToGetOrdersByCustomerResponse(this GetOrdersByCustomerResult result)
        {
            return new GetOrdersByCustomerResponse(result.Orders);
        }
    }
}

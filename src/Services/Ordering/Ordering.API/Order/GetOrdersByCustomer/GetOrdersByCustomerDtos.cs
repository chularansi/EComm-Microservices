using Ordering.Application.Dtos;

namespace Ordering.API.Order.GetOrdersByCustomer
{
    //public record GetOrdersByCustomerRequest(Guid CustomerId);
    public record GetOrdersByCustomerResponse(IEnumerable<OrderDto> Orders);
}

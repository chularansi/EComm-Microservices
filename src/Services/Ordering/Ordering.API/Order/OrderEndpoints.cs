using Ordering.API.Order.CreateOrder;
using Ordering.API.Order.DeleteOrder;
using Ordering.API.Order.GetOrders;
using Ordering.API.Order.GetOrdersByCustomer;
using Ordering.API.Order.GetOrdersByName;
using Ordering.API.Order.UpdateOrder;

namespace Ordering.API.Order
{
    public static class OrderEndpoints
    {
        public static void MapOrderEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/orders");

            group.CreateOrder();
            group.DeleteOrder();
            group.UpdateOrder();
            group.GetOrdersByName();
            group.GetOrdersByCustomer();
            group.GetOrders();
        }
    }
}

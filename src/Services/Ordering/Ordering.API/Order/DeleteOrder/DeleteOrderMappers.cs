using Ordering.Application.Orders.DeleteOrder;

namespace Ordering.API.Order.DeleteOrder
{
    public static class DeleteOrderMappers
    {
        //public static DeleteOrderCommand ToDeleteOrderCommand(this DeleteOrderRequest request, Guid id)
        //{
        //    return new DeleteOrderCommand(id);
        //}
        public static DeleteOrderResponse ToDeleteOrderResponse(this DeleteOrderResult result)
        {
            return new DeleteOrderResponse(result.IsSuccess);
        }
    }
}
